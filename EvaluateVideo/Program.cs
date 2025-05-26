using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

class Program
{
    // Configuration parameters
    private static readonly int FRAME_PROCESSING_RATE = 15;      // Process every Nth frame
    private static readonly bool USE_MULTITHREADING = true;      // Enable parallel processing
    private static readonly bool DOWNSCALE_VIDEO = true;         // Reduce resolution for faster processing
    private static readonly double DOWNSCALE_FACTOR = 0.5;       // Resize factor (0.5 = half size)
    private static readonly bool DEBUG_MODE = true;              // Show detailed debugging info

    // Team jersey colors - adjusted for better detection
    private static readonly Scalar TEAM_A_LOWER = new Scalar(0, 0, 150);      // White team (TeamA)
    private static readonly Scalar TEAM_A_UPPER = new Scalar(180, 70, 255);

    private static readonly Scalar TEAM_B_LOWER = new Scalar(95, 100, 30);    // Dark blue team (TeamB)
    private static readonly Scalar TEAM_B_UPPER = new Scalar(145, 255, 190);

    // Ball detection - adjusted for orange basketball
    private static readonly Scalar BALL_LOWER = new Scalar(0, 100, 100);      // Orange basketball (wider range)
    private static readonly Scalar BALL_UPPER = new Scalar(30, 255, 255);

    // Track game state
    private static readonly int POINTS_PER_BASKET = 2;      // Regular basket is worth 2 points
    private static readonly int MAX_TOTAL_SCORE = 30;       // Reasonable max for 10-minute game
    private static readonly int EXPECTED_BASKETS_PER_MINUTE = 1; // Average pace of scoring

    // Scoring detection sensitivity
    private static readonly int SCORE_THRESHOLD = 3;         // Number of detections needed to confirm score
    private static readonly int COOLDOWN_FRAMES = 60;        // Frames to skip after detecting a basket
    private static readonly double TEAM_DETECTION_RATIO = 0.6; // Required majority for team detection
    private static readonly TimeSpan MIN_TIME_BETWEEN_SCORES = TimeSpan.FromSeconds(7); // More realistic for pickup game

    static void Main(string[] args)
    {
        Console.WriteLine("🏀 Basketball Game Scoring Detection");
        Console.WriteLine("-----------------------------------");

        string videoPath = args.Length > 0 ? args[0] : @"C:\Videos\pickup_game.mp4";

        if (!File.Exists(videoPath))
        {
            Console.WriteLine($"❌ Error: Video file not found at {videoPath}");
            Console.WriteLine("Please provide a valid video file path as the first argument.");
            return;
        }

        string tempFramePath = Path.Combine(Path.GetDirectoryName(videoPath), "temp_frame.jpg");

        try
        {
            ProcessVideo(videoPath, tempFramePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void ProcessVideo(string videoPath, string tempFramePath)
    {
        Stopwatch totalTime = Stopwatch.StartNew();

        // Team scores
        int teamAScore = 0;  // White team
        int teamBScore = 0;  // Dark blue team

        // Scoring detection variables
        Queue<string> recentDetections = new Queue<string>();
        bool potentialScoringEvent = false;
        int framesSincePotential = 0;
        int scoringCooldown = 0;
        DateTime lastScoringTime = DateTime.MinValue;
        HashSet<int> detectedBasketFrames = new HashSet<int>();

        // Game timing variables
        double totalGameSeconds = 10 * 60; // 10 minutes
        double currentGameTime = 0;
        int totalBaskets = 0;

        // Set up video capture
        using var capture = new VideoCapture(videoPath);
        if (!capture.IsOpened())
        {
            Console.WriteLine("❌ Unable to open video file.");
            return;
        }

        // Get video properties
        double fps = capture.Get(VideoCaptureProperties.Fps);
        int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
        int frameSkip = Math.Max(1, (int)(fps / FRAME_PROCESSING_RATE));
        int width = (int)capture.Get(VideoCaptureProperties.FrameWidth);
        int height = (int)capture.Get(VideoCaptureProperties.FrameHeight);

        // Calculate new dimensions if downscaling
        int newWidth = DOWNSCALE_VIDEO ? (int)(width * DOWNSCALE_FACTOR) : width;
        int newHeight = DOWNSCALE_VIDEO ? (int)(height * DOWNSCALE_FACTOR) : height;

        Console.WriteLine($"🎥 Video stats: {width}x{height}, {fps} FPS, {totalFrames} frames");
        Console.WriteLine($"⚙️ Processing every {frameSkip}th frame, resolution: {newWidth}x{newHeight}");
        Console.WriteLine($"⏱️ Estimated completion time: {EstimateCompletionTime(totalFrames, frameSkip, fps)} minutes");

        // Court regions
        Rect topBasketRegion = new Rect();
        Rect bottomBasketRegion = new Rect();
        Dictionary<string, Mat> masks = new Dictionary<string, Mat>();
        bool regionsInitialized = false;

        Console.WriteLine("🏀 Processing video...");

        // Manually manage frame object to reduce memory allocations
        using var frame = new Mat();
        int frameCount = 0;
        Mat previousFrame = null;

        // Process frame counter (actual frames we analyze)
        int processedFrameCount = 0;

        // Start processing frames
        while (true)
        {
            // Read next frame
            capture.Read(frame);
            if (frame.Empty()) break;

            frameCount++;

            // Update game time with this frame
            currentGameTime = (double)frameCount / fps;

            // Skip frames for performance
            if (frameCount % frameSkip != 0) continue;

            processedFrameCount++;

            // Status update every 1000 processed frames
            if (processedFrameCount % 1000 == 0)
            {
                double progress = (double)frameCount / totalFrames * 100;
                long memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                // Update game time based on frame count and FPS
                currentGameTime = (double)frameCount / fps;
                double gameProgress = Math.Min(100, (currentGameTime / totalGameSeconds) * 100);

                Console.WriteLine($"📊 Progress: {progress:F1}% ({frameCount}/{totalFrames} frames)");
                Console.WriteLine($"⏱️ Game time: {TimeSpan.FromSeconds(currentGameTime):mm\\:ss} / 10:00 ({gameProgress:F1}%)");
                Console.WriteLine($"🏀 Current score: White {teamAScore} - {teamBScore} Blue (Baskets: {totalBaskets})");
                Console.WriteLine($"💾 Memory usage: {memoryUsage} MB");

                // Force garbage collection occasionally to keep memory usage down
                GC.Collect();
            }

            // Use a single resized frame to save memory
            Mat processedFrame = null;

            try
            {
                // Resize for performance if enabled
                if (DOWNSCALE_VIDEO)
                {
                    processedFrame = new Mat();
                    Cv2.Resize(frame, processedFrame, new Size(newWidth, newHeight));
                }
                else
                {
                    processedFrame = frame.Clone();
                }

                // Initialize basket regions on first processed frame
                if (!regionsInitialized)
                {
                    InitializeCourtRegions(processedFrame, out topBasketRegion, out bottomBasketRegion, ref masks);
                    regionsInitialized = true;
                }

                // Store current frame for motion comparison, release previous if exists
                if (previousFrame != null)
                {
                    previousFrame.Dispose();
                }
                previousFrame = processedFrame.Clone();

                // Process scoring with cooldown mechanism
                ProcessScoring(
                    processedFrame, frame, tempFramePath, frameCount,
                    topBasketRegion, bottomBasketRegion, masks,
                    ref teamAScore, ref teamBScore,
                    ref potentialScoringEvent, ref framesSincePotential,
                    ref scoringCooldown, ref lastScoringTime,
                    recentDetections, detectedBasketFrames,
                    ref totalBaskets, currentGameTime
                );
            }
            finally
            {
                // Make sure to release processedFrame if it was created
                if (processedFrame != null && processedFrame != frame)
                {
                    processedFrame.Dispose();
                }
            }
        }

        // Clean up remaining resources
        if (previousFrame != null)
        {
            previousFrame.Dispose();
        }

        // Dispose all masks
        foreach (var mask in masks)
        {
            mask.Value.Dispose();
        }

        totalTime.Stop();

        Console.WriteLine("\n🏁 Game Summary:");
        Console.WriteLine($"Game duration: {TimeSpan.FromSeconds(currentGameTime):mm\\:ss} minutes");
        Console.WriteLine($"Total baskets: {totalBaskets}");
        Console.WriteLine($"Scoring pace: {totalBaskets / (currentGameTime / 60):F1} baskets per minute");
        Console.WriteLine("\n✅ Final Score:");
        Console.WriteLine($"White team (A): {teamAScore}");
        Console.WriteLine($"Dark blue team (B): {teamBScore}");
        Console.WriteLine(teamAScore > teamBScore ? "🏆 White team Wins!" :
                          teamBScore > teamAScore ? "🏆 Dark blue team Wins!" : "🤝 It's a Tie!");
        Console.WriteLine($"\n⏱️ Total processing time: {totalTime.Elapsed.TotalMinutes:F1} minutes");
    }

    static void ProcessScoring(
        Mat processedFrame, Mat originalFrame, string tempFramePath, int frameCount,
        Rect topBasketRegion, Rect bottomBasketRegion, Dictionary<string, Mat> masks,
        ref int teamAScore, ref int teamBScore,
        ref bool potentialScoringEvent, ref int framesSincePotential,
        ref int scoringCooldown, ref DateTime lastScoringTime,
        Queue<string> recentDetections, HashSet<int> detectedBasketFrames,
        ref int totalBaskets, double currentGameTime)
    {
        // Only process scoring if not in cooldown
        if (scoringCooldown > 0)
        {
            scoringCooldown--;
            if (DEBUG_MODE && scoringCooldown % 10 == 0)
            {
                Console.WriteLine($"Cooling down: {scoringCooldown} frames left");
            }
            return;
        }

        // FAST PATH: Enhanced ball detection near baskets
        bool ballNearBasket = false;
        bool ballInTopBasket = false;
        bool ballInBottomBasket = false;

        using (var topRegion = new Mat(processedFrame, topBasketRegion))
        using (var bottomRegion = new Mat(processedFrame, bottomBasketRegion))
        {
            ballInTopBasket = IsBasketballInRegion(topRegion);
            ballInBottomBasket = IsBasketballInRegion(bottomRegion);
            ballNearBasket = ballInTopBasket || ballInBottomBasket;

            // Save which basket the ball is near for scoring direction
            if (ballNearBasket && DEBUG_MODE)
            {
                Console.WriteLine($"Ball detected near: {(ballInTopBasket ? "TOP" : "BOTTOM")} basket");
            }
        }

        // Only proceed with detailed analysis if ball is near basket
        if (ballNearBasket)
        {
            potentialScoringEvent = true;
            framesSincePotential = 0;
        }

        // Process potential scoring events with strict validation
        if (potentialScoringEvent)
        {
            framesSincePotential++;

            // Only add frame to detection list if not already present
            if (!detectedBasketFrames.Contains(frameCount))
            {
                detectedBasketFrames.Add(frameCount);
            }

            // Store which basket the ball was detected in
            string basketLocation = ballInTopBasket ? "TopBasket" : "BottomBasket";

            // Wait a few frames to see if the ball goes through the hoop
            if (framesSincePotential >= 3 && framesSincePotential <= 10)
            {
                string result = "Unknown";

                // For the middle frame in the sequence, use Python for advanced detection
                if (framesSincePotential == 5)
                {
                    // Save full resolution frame for Python processing
                    Cv2.ImWrite(tempFramePath, originalFrame);
                    result = RunPythonScoreDetection(tempFramePath);
                    if (DEBUG_MODE) Console.WriteLine($"Python detection result: {result}");
                }

                // If Python is uncertain, use jersey detection
                if (result != "TeamA" && result != "TeamB")
                {
                    result = OptimizedJerseyDetection(processedFrame, masks);
                }

                // Add basket location to help determine who scored
                if (result != "Unknown")
                {
                    string detection = $"{result}_{basketLocation}";
                    recentDetections.Enqueue(detection);
                    if (DEBUG_MODE) Console.WriteLine($"Added detection: {detection}");
                }

                // Keep queue at reasonable size
                while (recentDetections.Count > SCORE_THRESHOLD)
                    recentDetections.Dequeue();

                // Group detections by team to handle mixed signals
                int teamACount = recentDetections.Count(d => d.StartsWith("TeamA"));
                int teamBCount = recentDetections.Count(d => d.StartsWith("TeamB"));

                if (DEBUG_MODE && (teamACount > 0 || teamBCount > 0))
                {
                    Console.WriteLine($"Current detection counts - TeamA: {teamACount}, TeamB: {teamBCount}");
                }

                // Check if we have enough evidence of one team scoring AND enough time has passed since last score
                TimeSpan timeSinceLastScore = DateTime.Now - lastScoringTime;
                bool enoughTimePassed = timeSinceLastScore >= MIN_TIME_BETWEEN_SCORES;

                if (recentDetections.Count >= SCORE_THRESHOLD && enoughTimePassed)
                {
                    // Team A scoring detection
                    if (teamACount >= SCORE_THRESHOLD * TEAM_DETECTION_RATIO && teamACount > teamBCount)
                    {
                        // Check if we're within realistic scoring pace for a 10-minute game
                        double minutesElapsed = currentGameTime / 60.0;
                        double currentPace = (totalBaskets + 1) / Math.Max(1, minutesElapsed);
                        bool paceIsRealistic = currentPace <= (EXPECTED_BASKETS_PER_MINUTE * 2); // Allow 2x average pace

                        // Check if total score would exceed maximum safeguard
                        if (teamAScore + teamBScore + POINTS_PER_BASKET <= MAX_TOTAL_SCORE && paceIsRealistic)
                        {
                            teamAScore += POINTS_PER_BASKET;
                            totalBaskets++;
                            Console.WriteLine($"🎯 White team (A) scores! Total: {teamAScore}");
                            Console.WriteLine($"   Game time: {TimeSpan.FromSeconds(currentGameTime):mm\\:ss}, Basket #{totalBaskets}");

                            // Update last scoring time and activate cooldown
                            lastScoringTime = DateTime.Now;
                            scoringCooldown = COOLDOWN_FRAMES;

                            // Reset after scoring
                            potentialScoringEvent = false;
                            recentDetections.Clear();
                        }
                        else if (!paceIsRealistic)
                        {
                            Console.WriteLine($"⚠️ Scoring pace too high ({currentPace:F1}/min) - ignoring potential basket");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Maximum expected score reached - ignoring potential basket");
                        }
                    }
                    // Team B scoring detection
                    else if (teamBCount >= SCORE_THRESHOLD * TEAM_DETECTION_RATIO && teamBCount > teamACount)
                    {
                        // Check if we're within realistic scoring pace for a 10-minute game
                        double minutesElapsed = currentGameTime / 60.0;
                        double currentPace = (totalBaskets + 1) / Math.Max(1, minutesElapsed);
                        bool paceIsRealistic = currentPace <= (EXPECTED_BASKETS_PER_MINUTE * 2); // Allow 2x average pace

                        // Check if total score would exceed maximum safeguard
                        if (teamBScore + teamAScore + POINTS_PER_BASKET <= MAX_TOTAL_SCORE && paceIsRealistic)
                        {
                            teamBScore += POINTS_PER_BASKET;
                            totalBaskets++;
                            Console.WriteLine($"🎯 Dark blue team (B) scores! Total: {teamBScore}");
                            Console.WriteLine($"   Game time: {TimeSpan.FromSeconds(currentGameTime):mm\\:ss}, Basket #{totalBaskets}");

                            // Update last scoring time and activate cooldown
                            lastScoringTime = DateTime.Now;
                            scoringCooldown = COOLDOWN_FRAMES;

                            // Reset after scoring
                            potentialScoringEvent = false;
                            recentDetections.Clear();
                        }
                        else if (!paceIsRealistic)
                        {
                            Console.WriteLine($"⚠️ Scoring pace too high ({currentPace:F1}/min) - ignoring potential basket");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Maximum expected score reached - ignoring potential basket");
                        }
                    }
                }
            }

            // Reset if no score detected after certain frames
            if (framesSincePotential > 15)
            {
                if (DEBUG_MODE) Console.WriteLine("Resetting potential scoring event");
                potentialScoringEvent = false;
                recentDetections.Clear();
            }
        }
    }

    static void InitializeCourtRegions(Mat frame, out Rect topBasket, out Rect bottomBasket, ref Dictionary<string, Mat> masks)
    {
        int w = frame.Width;
        int h = frame.Height;

        // Define basket regions - made slightly larger to ensure detection
        topBasket = new Rect(
            (int)(w * 0.35),   // Move left edge further left
            (int)(h * 0.05),
            (int)(w * 0.3),    // Make wider
            (int)(h * 0.25)
        );

        bottomBasket = new Rect(
            (int)(w * 0.35),   // Move left edge further left
            (int)(h * 0.7),
            (int)(w * 0.3),    // Make wider
            (int)(h * 0.25)
        );

        // Create court masks for optimization
        masks = new Dictionary<string, Mat>();

        // Create mask for court
        Mat courtMask = new Mat(h, w, MatType.CV_8UC1, new Scalar(0));

        // Draw filled rectangle covering the main court area
        Rect courtRect = new Rect(
            (int)(w * 0.1),
            (int)(h * 0.1),
            (int)(w * 0.8),
            (int)(h * 0.8)
        );

        Cv2.Rectangle(courtMask, courtRect, new Scalar(255), -1);
        masks["court"] = courtMask;

        // Create separate team areas if needed for better scoring detection
        Mat teamAAreaMask = new Mat(h, w, MatType.CV_8UC1, new Scalar(0));
        Rect teamARect = new Rect(
            (int)(w * 0.1),
            (int)(h * 0.1),
            (int)(w * 0.4),
            (int)(h * 0.8)
        );
        Cv2.Rectangle(teamAAreaMask, teamARect, new Scalar(255), -1);
        masks["teamA_area"] = teamAAreaMask;

        Mat teamBAreaMask = new Mat(h, w, MatType.CV_8UC1, new Scalar(0));
        Rect teamBRect = new Rect(
            (int)(w * 0.5),
            (int)(h * 0.1),
            (int)(w * 0.4),
            (int)(h * 0.8)
        );
        Cv2.Rectangle(teamBAreaMask, teamBRect, new Scalar(255), -1);
        masks["teamB_area"] = teamBAreaMask;

        Console.WriteLine("🏀 Court regions initialized");
        Console.WriteLine($"Top basket region: {topBasket}");
        Console.WriteLine($"Bottom basket region: {bottomBasket}");
    }

    static bool IsBasketballInRegion(Mat region)
    {
        // Local copies of the static fields for use in this method
        Scalar ballLower = BALL_LOWER;
        Scalar ballUpper = BALL_UPPER;
        bool debugMode = DEBUG_MODE;

        // More restrictive ball detection with strict size and shape constraints
        using (var hsv = new Mat())
        {
            Cv2.CvtColor(region, hsv, ColorConversionCodes.BGR2HSV);

            using (var ballMask = new Mat())
            {
                Cv2.InRange(hsv, ballLower, ballUpper, ballMask);

                // Apply morphological operations to clean up noise
                using (var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3)))
                {
                    Cv2.MorphologyEx(ballMask, ballMask, MorphTypes.Open, kernel);
                }

                // Find contours of potential ball
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(ballMask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // Look for circular-ish objects of appropriate size
                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    // Stricter size constraints for basketball
                    if (area > 50 && area < 400) // Adjusted size range
                    {
                        // Check circularity - basketballs are very circular
                        double perimeter = Cv2.ArcLength(contour, true);
                        double circularity = 4 * Math.PI * area / (perimeter * perimeter);

                        // More strict circularity requirement
                        if (circularity > 0.7) // Higher threshold for circularity
                        {
                            // Additional validation: Get bounding box and check aspect ratio
                            Rect boundingBox = Cv2.BoundingRect(contour);
                            double aspectRatio = (double)boundingBox.Width / boundingBox.Height;

                            // A perfect circle has aspect ratio of 1.0
                            if (aspectRatio > 0.8 && aspectRatio < 1.2)
                            {
                                if (debugMode)
                                {
                                    Console.WriteLine($"Ball detected: Area={area:F1}, Circularity={circularity:F2}, Aspect={aspectRatio:F2}");
                                }
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }
    }

    static string RunPythonScoreDetection(string framePath)
    {
        // Local copy of DEBUG_MODE
        bool debugMode = DEBUG_MODE;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"score_detect.py \"{framePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            // Set timeout to prevent long-running Python script
            if (!process.WaitForExit(1500)) // Increased timeout to 1.5 seconds
            {
                try { process.Kill(); } catch { }
                Console.WriteLine("⚠️ Python detection timed out");
                return "Unknown";
            }

            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd().Trim();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"⚠️ Python error: {error}");
                return "Unknown";
            }

            if (output == "TeamA" || output == "TeamB")
            {
                Console.WriteLine($"✅ Python detected: {output}");
            }

            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error running Python script: {ex.Message}");
            return "Unknown";
        }
    }

    static string OptimizedJerseyDetection(Mat frame, Dictionary<string, Mat> masks)
    {
        // Local copies of static fields for use in this method
        Scalar teamALower = TEAM_A_LOWER;
        Scalar teamAUpper = TEAM_A_UPPER;
        Scalar teamBLower = TEAM_B_LOWER;
        Scalar teamBUpper = TEAM_B_UPPER;
        bool debugMode = DEBUG_MODE;

        using (var courtOnly = new Mat())
        using (var hsv = new Mat())
        using (var whiteMask = new Mat())
        using (var blueMask = new Mat())
        {
            // Apply court mask to eliminate background
            frame.CopyTo(courtOnly, masks["court"]);

            // Convert to HSV for color detection
            Cv2.CvtColor(courtOnly, hsv, ColorConversionCodes.BGR2HSV);

            // Detect white jerseys (Team A) - adjusted thresholds
            Cv2.InRange(hsv, teamALower, teamAUpper, whiteMask);

            // Detect dark blue jerseys (Team B) - adjusted thresholds
            Cv2.InRange(hsv, teamBLower, teamBUpper, blueMask);

            // Use better kernel for morphological operations
            using (var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)))
            {
                // Apply more aggressive morphological operations
                Cv2.MorphologyEx(whiteMask, whiteMask, MorphTypes.Open, kernel);
                Cv2.MorphologyEx(blueMask, blueMask, MorphTypes.Open, kernel);

                // Dilate to connect jersey parts
                Cv2.Dilate(whiteMask, whiteMask, kernel);
                Cv2.Dilate(blueMask, blueMask, kernel);
            }

            // Find contours to count actual players rather than just pixels
            Point[][] whiteContours, blueContours;
            HierarchyIndex[] whiteHierarchy, blueHierarchy;

            Cv2.FindContours(whiteMask, out whiteContours, out whiteHierarchy,
                             RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(blueMask, out blueContours, out blueHierarchy,
                             RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Count large contours (likely to be players)
            int whitePlayerCount = 0;
            int bluePlayerCount = 0;

            foreach (var contour in whiteContours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > 500) // Minimum area to be considered a player
                {
                    whitePlayerCount++;
                }
            }

            foreach (var contour in blueContours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > 500) // Minimum area to be considered a player
                {
                    bluePlayerCount++;
                }
            }

            // Also count total pixels as a fallback
            int whitePixels = Cv2.CountNonZero(whiteMask);
            int bluePixels = Cv2.CountNonZero(blueMask);

            if (debugMode)
            {
                Console.WriteLine($"🔍 White players: {whitePlayerCount}, Blue players: {bluePlayerCount}");
                Console.WriteLine($"🔍 White pixels: {whitePixels}, Blue pixels: {bluePixels}");
            }

            // Decision logic - prioritize player count, fall back to pixel count
            // First check if we have meaningful player detection
            if (whitePlayerCount > 2 && whitePlayerCount > bluePlayerCount * 1.5)
                return "TeamA"; // White team
            else if (bluePlayerCount > 2 && bluePlayerCount > whitePlayerCount * 1.5)
                return "TeamB"; // Dark blue team

            // Fall back to pixel-based detection with more aggressive thresholds
            if (whitePixels > bluePixels * 3 && whitePixels > 2000)
                return "TeamA"; // White team
            else if (bluePixels > whitePixels * 3 && bluePixels > 2000)
                return "TeamB"; // Dark blue team

            return "Unknown";
        }
    }

    static string EstimateCompletionTime(int totalFrames, int frameSkip, double fps)
    {
        // Estimate based on typical processing speeds
        double framesPerSecond = 10; // Typical processing speed on mid-range hardware
        double totalProcessedFrames = totalFrames / frameSkip;
        double estimatedSeconds = totalProcessedFrames / framesPerSecond;

        return (estimatedSeconds / 60).ToString("F1");
    }
}