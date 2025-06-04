using OpenCvSharp;
using OpenCvSharp.Tracking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// NuGet packages required:
// - OpenCvSharp4
// - OpenCvSharp4.runtime.win (or appropriate runtime for your OS)
// - OpenCvSharp4.Extensions (for Bitmap conversions if needed)

namespace BasketballGameAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🏀 Advanced Basketball Game Analyzer");
            Console.WriteLine("=====================================");

            string videoPath = args.Length > 0 ? args[0] : @"C:\Videos\pickup_game.mp4";

            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"❌ Error: Video file not found at {videoPath}");
                Console.WriteLine("Usage: BasketballAnalyzer.exe <video_path>");
                return;
            }

            var config = new AnalyzerConfig();
            var analyzer = new GameAnalyzer(config);

            try
            {
                var result = await analyzer.AnalyzeGameAsync(videoPath);
                DisplayResults(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error: {ex.Message}");
                if (config.DebugMode)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        static void DisplayResults(GameResult result)
        {
            Console.WriteLine("\n🏁 GAME ANALYSIS COMPLETE");
            Console.WriteLine("=========================");
            Console.WriteLine($"📊 Game Duration: {result.Duration:mm\\:ss}");
            Console.WriteLine($"🎯 Total Baskets: {result.TotalBaskets}");
            Console.WriteLine($"⚡ Scoring Pace: {result.ScoringPace:F1} baskets/min");
            Console.WriteLine($"🎮 Ball Possession: Team A {result.TeamAPossession:F0}% - Team B {result.TeamBPossession:F0}%");

            Console.WriteLine("\n📈 FINAL SCORE:");
            Console.WriteLine($"   Team A (White): {result.TeamAScore}");
            Console.WriteLine($"   Team B (Blue):  {result.TeamBScore}");

            if (result.TeamAScore > result.TeamBScore)
                Console.WriteLine("\n🏆 TEAM A (WHITE) WINS!");
            else if (result.TeamBScore > result.TeamAScore)
                Console.WriteLine("\n🏆 TEAM B (BLUE) WINS!");
            else
                Console.WriteLine("\n🤝 IT'S A TIE!");

            Console.WriteLine("\n🎬 KEY MOMENTS:");
            foreach (var moment in result.KeyMoments.Take(10))
            {
                Console.WriteLine($"   {moment.GameTime:mm\\:ss} - {moment.Description}");
            }

            Console.WriteLine($"\n⚙️ Processing Stats:");
            Console.WriteLine($"   Total Frames: {result.TotalFramesProcessed:N0}");
            Console.WriteLine($"   Processing Time: {result.ProcessingTime:mm\\:ss}");
            Console.WriteLine($"   FPS: {result.ProcessingFPS:F1}");
            Console.WriteLine($"   Confidence Score: {result.ConfidenceScore:F1}%");
        }
    }

    public class AnalyzerConfig
    {
        // Performance settings
        public int FrameSkip { get; set; } = 15;
        public bool UseMultithreading { get; set; } = true;
        public int MaxThreads { get; set; } = Environment.ProcessorCount;
        public bool EnableGpuAcceleration { get; set; } = false;

        // Video processing
        public bool DownscaleVideo { get; set; } = true;
        public double DownscaleFactor { get; set; } = 0.5;
        public int BufferSize { get; set; } = 100;

        // Detection parameters
        public int ScoreConfirmationThreshold { get; set; } = 3;
        public int CooldownFrames { get; set; } = 60;
        public double TeamDetectionRatio { get; set; } = 0.6;
        public TimeSpan MinTimeBetweenScores { get; set; } = TimeSpan.FromSeconds(5);

        // Game rules
        public int PointsPerBasket { get; set; } = 2;
        public int PointsPerThreePointer { get; set; } = 3;
        public int MaxExpectedScore { get; set; } = 50;
        public double ExpectedBasketsPerMinute { get; set; } = 2.0;

        // Debug settings
        public bool DebugMode { get; set; } = false;
        public bool SaveDebugFrames { get; set; } = false;
        public string DebugOutputPath { get; set; } = "./debug";

        // Color detection ranges (HSV)
        public ColorRange TeamAColors { get; set; } = new ColorRange
        {
            Lower = new Scalar(0, 0, 150),    // White
            Upper = new Scalar(180, 70, 255)
        };

        public ColorRange TeamBColors { get; set; } = new ColorRange
        {
            Lower = new Scalar(95, 100, 30),   // Dark Blue
            Upper = new Scalar(145, 255, 190)
        };

        public ColorRange BallColors { get; set; } = new ColorRange
        {
            Lower = new Scalar(0, 100, 100),   // Orange
            Upper = new Scalar(30, 255, 255)
        };
    }

    public class ColorRange
    {
        public Scalar Lower { get; set; }
        public Scalar Upper { get; set; }
    }

    public class GameAnalyzer
    {
        private readonly AnalyzerConfig config;
        private readonly BallTracker ballTracker;
        private readonly PlayerTracker playerTracker;
        private readonly ScoringDetector scoringDetector;
        private readonly CourtAnalyzer courtAnalyzer;
        private readonly PerformanceMonitor perfMonitor;

        public GameAnalyzer(AnalyzerConfig config)
        {
            this.config = config;
            this.ballTracker = new BallTracker(config);
            this.playerTracker = new PlayerTracker(config);
            this.scoringDetector = new ScoringDetector(config);
            this.courtAnalyzer = new CourtAnalyzer(config);
            this.perfMonitor = new PerformanceMonitor();
        }

        public async Task<GameResult> AnalyzeGameAsync(string videoPath)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new GameResult();

            using var capture = new VideoCapture(videoPath);
            if (!capture.IsOpened())
            {
                throw new Exception($"Cannot open video file: {videoPath}");
            }

            // Get video properties
            var videoInfo = GetVideoInfo(capture);
            result.Duration = videoInfo.Duration;

            Console.WriteLine($"🎥 Video: {videoInfo.Width}x{videoInfo.Height}, {videoInfo.Fps:F1} FPS, {videoInfo.TotalFrames:N0} frames");
            Console.WriteLine($"⏱️  Duration: {videoInfo.Duration:mm\\:ss}");

            // Initialize court detection
            courtAnalyzer.Initialize(videoInfo.Width, videoInfo.Height);

            // Process video
            if (config.UseMultithreading)
            {
                await ProcessVideoParallelAsync(capture, videoInfo, result);
            }
            else
            {
                await ProcessVideoSequentialAsync(capture, videoInfo, result);
            }

            // Calculate final statistics
            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            result.ProcessingFPS = result.TotalFramesProcessed / stopwatch.Elapsed.TotalSeconds;
            result.ScoringPace = result.TotalBaskets / (result.Duration.TotalMinutes);
            result.ConfidenceScore = CalculateConfidenceScore(result);

            return result;
        }

        private async Task ProcessVideoParallelAsync(VideoCapture capture, VideoInfo videoInfo, GameResult result)
        {
            var frameBuffer = new BlockingCollection<FrameData>(config.BufferSize);
            var cancellationToken = new CancellationTokenSource();

            // Start frame reader task
            var readerTask = Task.Run(() => ReadFrames(capture, videoInfo, frameBuffer, cancellationToken.Token));

            // Start processing tasks
            var processingTasks = new Task[config.MaxThreads];
            for (int i = 0; i < config.MaxThreads; i++)
            {
                processingTasks[i] = Task.Run(() => ProcessFrames(frameBuffer, videoInfo, result, cancellationToken.Token));
            }

            // Wait for completion
            await readerTask;
            frameBuffer.CompleteAdding();
            await Task.WhenAll(processingTasks);
        }

        private void ReadFrames(VideoCapture capture, VideoInfo videoInfo, BlockingCollection<FrameData> buffer, CancellationToken token)
        {
            int frameNumber = 0;

            while (!token.IsCancellationRequested && capture.IsOpened())
            {
                var frame = capture.RetrieveMat();
                if (frame == null || frame.Empty()) break;

                frameNumber++;

                // Skip frames based on config
                if (frameNumber % config.FrameSkip != 0)
                {
                    frame.Dispose();
                    continue;
                }

                var frameData = new FrameData
                {
                    Frame = frame.Clone(),
                    FrameNumber = frameNumber,
                    GameTime = TimeSpan.FromSeconds(frameNumber / videoInfo.Fps)
                };

                frame.Dispose();

                buffer.TryAdd(frameData, 100, token);

                // Progress update
                if (frameNumber % 1000 == 0)
                {
                    var progress = (double)frameNumber / videoInfo.TotalFrames * 100;
                    Console.WriteLine($"📊 Reading: {progress:F1}% ({frameNumber:N0}/{videoInfo.TotalFrames:N0})");
                }
            }
        }

        private void ProcessFrames(BlockingCollection<FrameData> buffer, VideoInfo videoInfo, GameResult result, CancellationToken token)
        {
            var localBallPositions = new List<BallPosition>();
            var localPlayerData = new List<PlayerData>();
            var localScoringEvents = new List<ScoringEvent>();

            foreach (var frameData in buffer.GetConsumingEnumerable(token))
            {
                try
                {
                    ProcessSingleFrame(frameData, videoInfo, localBallPositions, localPlayerData, localScoringEvents);

                    //Interlocked.Increment(ref result.TotalFramesProcessed);

                    // Periodic sync
                    if (result.TotalFramesProcessed % 100 == 0)
                    {
                        SyncResults(result, localBallPositions, localPlayerData, localScoringEvents);
                        localBallPositions.Clear();
                        localPlayerData.Clear();
                        localScoringEvents.Clear();
                    }
                }
                finally
                {
                    frameData.Frame?.Dispose();
                }
            }

            // Final sync
            SyncResults(result, localBallPositions, localPlayerData, localScoringEvents);
        }

        private async Task ProcessVideoSequentialAsync(VideoCapture capture, VideoInfo videoInfo, GameResult result)
        {
            int frameNumber = 0;

            var ballPositions = new List<BallPosition>();
            var playerData = new List<PlayerData>();
            var scoringEvents = new List<ScoringEvent>();

            while (capture.IsOpened())
            {
                var frame = capture.RetrieveMat();
                if (frame == null || frame.Empty()) break;

                frameNumber++;

                // Skip frames
                if (frameNumber % config.FrameSkip != 0)
                {
                    frame.Dispose();
                    continue;
                }

                var frameData = new FrameData
                {
                    Frame = frame.Clone(),
                    FrameNumber = frameNumber,
                    GameTime = TimeSpan.FromSeconds(frameNumber / videoInfo.Fps)
                };

                ProcessSingleFrame(frameData, videoInfo, ballPositions, playerData, scoringEvents);

                result.TotalFramesProcessed++;

                // Progress update
                if (frameNumber % 1000 == 0)
                {
                    var progress = (double)frameNumber / videoInfo.TotalFrames * 100;
                    var gameTime = frameData.GameTime;
                    Console.WriteLine($"📊 Progress: {progress:F1}% | Game Time: {gameTime:mm\\:ss} | Score: A {result.TeamAScore} - {result.TeamBScore} B");

                    // Sync results
                    SyncResults(result, ballPositions, playerData, scoringEvents);
                    ballPositions.Clear();
                    playerData.Clear();
                    scoringEvents.Clear();
                }

                frame.Dispose();
                frameData.Frame.Dispose();
            }

            // Final sync
            SyncResults(result, ballPositions, playerData, scoringEvents);
        }

        private void ProcessSingleFrame(FrameData frameData, VideoInfo videoInfo,
            List<BallPosition> ballPositions, List<PlayerData> playerData, List<ScoringEvent> scoringEvents)
        {
            Mat processedFrame = null;

            try
            {
                // Resize if needed
                if (config.DownscaleVideo)
                {
                    processedFrame = new Mat();
                    var newSize = new OpenCvSharp.Size(
                        (int)(frameData.Frame.Width * config.DownscaleFactor),
                        (int)(frameData.Frame.Height * config.DownscaleFactor)
                    );
                    Cv2.Resize(frameData.Frame, processedFrame, newSize);
                }
                else
                {
                    processedFrame = frameData.Frame.Clone();
                }

                // Detect ball
                var ballPos = ballTracker.DetectBall(processedFrame, frameData.GameTime);
                if (ballPos != null)
                {
                    ballPositions.Add(ballPos);
                }

                // Detect players
                var players = playerTracker.DetectPlayers(processedFrame, frameData.GameTime);
                playerData.AddRange(players);

                // Check for scoring
                var scoringEvent = scoringDetector.CheckForScore(processedFrame, ballPos, players,
                    courtAnalyzer.TopBasketRegion, courtAnalyzer.BottomBasketRegion, frameData.GameTime);

                if (scoringEvent != null)
                {
                    scoringEvents.Add(scoringEvent);
                }

                // Performance monitoring
                perfMonitor.RecordFrame();
            }
            finally
            {
                if (processedFrame != null && processedFrame != frameData.Frame)
                {
                    processedFrame.Dispose();
                }
            }
        }

        private void SyncResults(GameResult result, List<BallPosition> ballPositions,
            List<PlayerData> playerData, List<ScoringEvent> scoringEvents)
        {
            lock (result)
            {
                // Update ball tracking data
                foreach (var pos in ballPositions)
                {
                    result.BallTrajectory.Add(pos);
                }

                // Update player tracking
                foreach (var player in playerData)
                {
                    if (!result.PlayerTracking.ContainsKey(player.PlayerId))
                    {
                        result.PlayerTracking[player.PlayerId] = new List<PlayerData>();
                    }
                    result.PlayerTracking[player.PlayerId].Add(player);
                }

                // Process scoring events
                foreach (var evt in scoringEvents)
                {
                    if (evt.Team == "TeamA")
                    {
                        result.TeamAScore += evt.Points;
                    }
                    else if (evt.Team == "TeamB")
                    {
                        result.TeamBScore += evt.Points;
                    }

                    result.TotalBaskets++;
                    result.KeyMoments.Add(new KeyMoment
                    {
                        GameTime = evt.GameTime,
                        Description = $"{evt.Team} scores {evt.Points} points",
                        Type = "Score"
                    });
                }

                // Calculate possession
                UpdatePossessionStats(result, playerData);
            }
        }

        private void UpdatePossessionStats(GameResult result, List<PlayerData> playerData)
        {
            var teamACounts = playerData.Count(p => p.Team == "TeamA");
            var teamBCounts = playerData.Count(p => p.Team == "TeamB");
            var total = teamACounts + teamBCounts;

            if (total > 0)
            {
                var currentAPossession = (double)teamACounts / total * 100;
                var currentBPossession = (double)teamBCounts / total * 100;

                // Running average
                result.TeamAPossession = (result.TeamAPossession * 0.95) + (currentAPossession * 0.05);
                result.TeamBPossession = (result.TeamBPossession * 0.95) + (currentBPossession * 0.05);
            }
        }

        private VideoInfo GetVideoInfo(VideoCapture capture)
        {
            return new VideoInfo
            {
                Width = (int)capture.Get(VideoCaptureProperties.FrameWidth),
                Height = (int)capture.Get(VideoCaptureProperties.FrameHeight),
                Fps = capture.Get(VideoCaptureProperties.Fps),
                TotalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount),
                Duration = TimeSpan.FromSeconds(capture.Get(VideoCaptureProperties.FrameCount) / capture.Get(VideoCaptureProperties.Fps))
            };
        }

        private double CalculateConfidenceScore(GameResult result)
        {
            double confidence = 100.0;

            // Reduce confidence for unusual scoring patterns
            if (result.ScoringPace > config.ExpectedBasketsPerMinute * 3)
                confidence -= 20;

            if (result.TeamAScore + result.TeamBScore > config.MaxExpectedScore)
                confidence -= 15;

            // Increase confidence for consistent detection
            if (result.BallTrajectory.Count > 1000)
                confidence += 10;

            return Math.Max(0, Math.Min(100, confidence));
        }
    }

    // Supporting classes
    public class BallTracker
    {
        private readonly AnalyzerConfig config;
        private readonly KalmanFilter kalmanFilter;
        private Point2f lastPosition;
        private bool isTracking;

        public BallTracker(AnalyzerConfig config)
        {
            this.config = config;
            this.kalmanFilter = CreateKalmanFilter();
        }

        private KalmanFilter CreateKalmanFilter()
        {
            var kf = new KalmanFilter(4, 2, 0);

            // State transition matrix - predicts next position based on current position and velocity
            // [x, y, vx, vy]
            kf.TransitionMatrix.SetIdentity();
            kf.TransitionMatrix.At<float>(0, 2) = 1;  // x += vx
            kf.TransitionMatrix.At<float>(1, 3) = 1;  // y += vy

            // Measurement matrix - we only measure x and y
            kf.MeasurementMatrix.SetTo(0);
            kf.MeasurementMatrix.At<float>(0, 0) = 1;
            kf.MeasurementMatrix.At<float>(1, 1) = 1;

            // Process noise
            kf.ProcessNoiseCov.SetIdentity(new Scalar(1e-4));

            // Measurement noise
            kf.MeasurementNoiseCov.SetIdentity(new Scalar(1e-1));

            // Error covariance
            kf.ErrorCovPost.SetIdentity();

            return kf;
        }

        public BallPosition DetectBall(Mat frame, TimeSpan gameTime)
        {
            using var hsv = new Mat();
            using var mask = new Mat();

            Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv, config.BallColors.Lower, config.BallColors.Upper, mask);

            // Noise reduction
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);

            // Find contours
            using var contourOutput = mask.Clone();
            Cv2.FindContours(contourOutput, out Point[][] contours, out HierarchyIndex[] hierarchy,
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            Point2f? detectedPosition = null;
            double maxScore = 0;

            foreach (var contour in contours)
            {
                var area = Cv2.ContourArea(contour);
                if (area < 50 || area > 500) continue;

                // Calculate circularity
                var perimeter = Cv2.ArcLength(contour, true);
                var circularity = 4 * Math.PI * area / (perimeter * perimeter);

                if (circularity < 0.7) continue;

                // Get center
                var moments = Cv2.Moments(contour);
                if (moments.M00 == 0) continue;

                var center = new Point2f(
                    (float)(moments.M10 / moments.M00),
                    (float)(moments.M01 / moments.M00)
                );

                // Score based on circularity and area
                var score = circularity * Math.Sqrt(area);

                if (score > maxScore)
                {
                    maxScore = score;
                    detectedPosition = center;
                }
            }

            // Update Kalman filter
            if (detectedPosition.HasValue)
            {
                if (!isTracking)
                {
                    // Initialize Kalman filter
                    var initialState = new Mat(4, 1, MatType.CV_32F);
                    initialState.At<float>(0, 0) = detectedPosition.Value.X;
                    initialState.At<float>(1, 0) = detectedPosition.Value.Y;
                    initialState.At<float>(2, 0) = 0;
                    initialState.At<float>(3, 0) = 0;
                    kalmanFilter.StatePost = initialState;
                    isTracking = true;
                }

                // Predict and update
                var prediction = kalmanFilter.Predict();
                var measurement = new Mat(2, 1, MatType.CV_32F);
                measurement.At<float>(0, 0) = detectedPosition.Value.X;
                measurement.At<float>(1, 0) = detectedPosition.Value.Y;

                kalmanFilter.Correct(measurement);

                lastPosition = new Point2f(
                    kalmanFilter.StatePost.At<float>(0),
                    kalmanFilter.StatePost.At<float>(1)
                );

                return new BallPosition
                {
                    Position = lastPosition,
                    GameTime = gameTime,
                    Confidence = maxScore / 100.0,
                    Velocity = new Point2f(
                        kalmanFilter.StatePost.At<float>(2),
                        kalmanFilter.StatePost.At<float>(3)
                    )
                };
            }
            else if (isTracking)
            {
                // Use prediction when detection fails
                var prediction = kalmanFilter.Predict();
                lastPosition = new Point2f(
                    prediction.At<float>(0),
                    prediction.At<float>(1)
                );

                return new BallPosition
                {
                    Position = lastPosition,
                    GameTime = gameTime,
                    Confidence = 0.5,
                    IsPredicted = true
                };
            }

            return null;
        }
    }

    public class PlayerTracker
    {
        private readonly AnalyzerConfig config;
        private readonly BackgroundSubtractorMOG2 bgSubtractor;
        private readonly Dictionary<int, TrackedPlayer> trackedPlayers;
        private int nextPlayerId;

        public PlayerTracker(AnalyzerConfig config)
        {
            this.config = config;
            this.bgSubtractor = BackgroundSubtractorMOG2.Create();
            this.trackedPlayers = new Dictionary<int, TrackedPlayer>();
            this.nextPlayerId = 1;
        }

        public List<PlayerData> DetectPlayers(Mat frame, TimeSpan gameTime)
        {
            var players = new List<PlayerData>();

            using var fgMask = new Mat();
            bgSubtractor.Apply(frame, fgMask);

            // Clean up the mask
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(fgMask, fgMask, MorphTypes.Open, kernel);
            Cv2.MorphologyEx(fgMask, fgMask, MorphTypes.Close, kernel);

            // Find contours
            using var contourOutput = fgMask.Clone();
            Cv2.FindContours(contourOutput, out Point[][] contours, out HierarchyIndex[] hierarchy,
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var detectedPlayers = new List<DetectedPlayer>();

            foreach (var contour in contours)
            {
                var area = Cv2.ContourArea(contour);
                if (area < 1000 || area > 10000) continue;

                var rect = Cv2.BoundingRect(contour);
                var aspectRatio = (double)rect.Height / rect.Width;

                // Filter by aspect ratio (humans are taller than wide)
                if (aspectRatio < 1.5 || aspectRatio > 4.0) continue;

                // Detect team by jersey color
                var team = DetectTeam(frame, rect);

                detectedPlayers.Add(new DetectedPlayer
                {
                    BoundingBox = rect,
                    Team = team,
                    Center = new OpenCvSharp.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
                });
            }

            // Match with tracked players
            UpdateTracking(detectedPlayers, gameTime);

            // Convert to PlayerData
            foreach (var tracked in trackedPlayers.Values.Where(t => t.IsActive))
            {
                players.Add(new PlayerData
                {
                    PlayerId = tracked.Id,
                    Team = tracked.Team,
                    Position = tracked.CurrentPosition,
                    GameTime = gameTime,
                    Velocity = tracked.Velocity,
                    BoundingBox = tracked.BoundingBox
                });
            }

            return players;
        }

        private string DetectTeam(Mat frame, OpenCvSharp.Rect playerRect)
        {
            // Extract player region
            using var playerRegion = new Mat(frame, playerRect);
            using var hsv = new Mat();

            Cv2.CvtColor(playerRegion, hsv, ColorConversionCodes.BGR2HSV);

            // Create masks for both teams
            using var teamAMask = new Mat();
            using var teamBMask = new Mat();

            Cv2.InRange(hsv, config.TeamAColors.Lower, config.TeamAColors.Upper, teamAMask);
            Cv2.InRange(hsv, config.TeamBColors.Lower, config.TeamBColors.Upper, teamBMask);

            // Count pixels
            var teamAPixels = Cv2.CountNonZero(teamAMask);
            var teamBPixels = Cv2.CountNonZero(teamBMask);

            if (teamAPixels > teamBPixels * 2)
                return "TeamA";
            else if (teamBPixels > teamAPixels * 2)
                return "TeamB";
            else
                return "Unknown";
        }

        private void UpdateTracking(List<DetectedPlayer> detectedPlayers, TimeSpan gameTime)
        {
            // Simple nearest neighbor tracking
            var matched = new HashSet<int>();

            foreach (var detected in detectedPlayers)
            {
                TrackedPlayer bestMatch = null;
                double minDistance = double.MaxValue;

                foreach (var tracked in trackedPlayers.Values.Where(t => !matched.Contains(t.Id)))
                {
                    var distance = Math.Sqrt(
                        Math.Pow(detected.Center.X - tracked.CurrentPosition.X, 2) +
                        Math.Pow(detected.Center.Y - tracked.CurrentPosition.Y, 2)
                    );

                    if (distance < minDistance && distance < 100) // Max distance threshold
                    {
                        minDistance = distance;
                        bestMatch = tracked;
                    }
                }

                if (bestMatch != null)
                {
                    // Update existing player
                    matched.Add(bestMatch.Id);
                    bestMatch.Update(detected.Center, detected.BoundingBox, detected.Team, gameTime);
                }
                else
                {
                    // Create new player
                    var newPlayer = new TrackedPlayer
                    {
                        Id = nextPlayerId++,
                        Team = detected.Team,
                        CurrentPosition = detected.Center,
                        BoundingBox = detected.BoundingBox,
                        LastSeen = gameTime,
                        IsActive = true
                    };
                    trackedPlayers[newPlayer.Id] = newPlayer;
                }
            }

            // Mark unmatched players as inactive
            foreach (var tracked in trackedPlayers.Values)
            {
                if (!matched.Contains(tracked.Id))
                {
                    tracked.IsActive = false;
                }
            }

            // Remove players not seen for a while
            var toRemove = trackedPlayers
                .Where(kvp => (gameTime - kvp.Value.LastSeen).TotalSeconds > 2)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in toRemove)
            {
                trackedPlayers.Remove(id);
            }
        }
    }

    public class ScoringDetector
    {
        private readonly AnalyzerConfig config;
        private readonly Queue<ScoringCandidate> candidates;
        private TimeSpan lastScoringTime;
        private int cooldownCounter;

        public ScoringDetector(AnalyzerConfig config)
        {
            this.config = config;
            this.candidates = new Queue<ScoringCandidate>();
            this.lastScoringTime = TimeSpan.Zero;
        }

        public ScoringEvent CheckForScore(Mat frame, BallPosition ballPos, List<PlayerData> players,
            Rect topBasket, Rect bottomBasket, TimeSpan gameTime)
        {
            // Check cooldown
            if (cooldownCounter > 0)
            {
                cooldownCounter--;
                return null;
            }

            if (ballPos == null) return null;

            // Check if ball is in basket region
            bool inTopBasket = topBasket.Contains(new Point((int)ballPos.Position.X, (int)ballPos.Position.Y));
            bool inBottomBasket = bottomBasket.Contains(new Point((int)ballPos.Position.X, (int)ballPos.Position.Y));

            if (!inTopBasket && !inBottomBasket) return null;

            // Determine which team likely scored based on player positions
            var nearbyPlayers = players
                .Where(p => Distance(p.Position, new Point((int)ballPos.Position.X, (int)ballPos.Position.Y)) < 200)
                .ToList();

            if (nearbyPlayers.Count == 0) return null;

            var teamACounts = nearbyPlayers.Count(p => p.Team == "TeamA");
            var teamBCounts = nearbyPlayers.Count(p => p.Team == "TeamB");

            string scoringTeam = null;
            if (teamACounts > teamBCounts * config.TeamDetectionRatio)
                scoringTeam = "TeamA";
            else if (teamBCounts > teamACounts * config.TeamDetectionRatio)
                scoringTeam = "TeamB";

            if (scoringTeam == null) return null;

            // Add to candidates
            candidates.Enqueue(new ScoringCandidate
            {
                Team = scoringTeam,
                GameTime = gameTime,
                Basket = inTopBasket ? "Top" : "Bottom",
                Confidence = (double)Math.Max(teamACounts, teamBCounts) / nearbyPlayers.Count
            });

            // Keep queue size manageable
            while (candidates.Count > config.ScoreConfirmationThreshold * 2)
                candidates.Dequeue();

            // Check if we have enough consistent detections
            var recentCandidates = candidates.Where(c => (gameTime - c.GameTime).TotalSeconds < 1).ToList();

            if (recentCandidates.Count >= config.ScoreConfirmationThreshold)
            {
                var teamVotes = recentCandidates.GroupBy(c => c.Team)
                    .Select(g => new { Team = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .First();

                if (teamVotes.Count >= config.ScoreConfirmationThreshold * config.TeamDetectionRatio)
                {
                    // Check minimum time between scores
                    if (gameTime - lastScoringTime >= config.MinTimeBetweenScores)
                    {
                        lastScoringTime = gameTime;
                        cooldownCounter = config.CooldownFrames;
                        candidates.Clear();

                        // Determine points (could check for 3-pointer based on position)
                        int points = config.PointsPerBasket;

                        return new ScoringEvent
                        {
                            Team = teamVotes.Team,
                            Points = points,
                            GameTime = gameTime,
                            Basket = recentCandidates.First().Basket,
                            //Confidence = recentCandidates.Average(c => c.Confidence)
                        };
                    }
                }
            }

            return null;
        }

        private double Distance(OpenCvSharp.Point p1, OpenCvSharp.Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }

    public class CourtAnalyzer
    {
        private readonly AnalyzerConfig config;
        public Rect TopBasketRegion { get; private set; }
        public Rect BottomBasketRegion { get; private set; }
        public Rect CourtBounds { get; private set; }

        public CourtAnalyzer(AnalyzerConfig config)
        {
            this.config = config;
        }

        public void Initialize(int frameWidth, int frameHeight)
        {
            // Define basket regions
            TopBasketRegion = new Rect(
                (int)(frameWidth * 0.35),
                (int)(frameHeight * 0.05),
                (int)(frameWidth * 0.3),
                (int)(frameHeight * 0.20)
            );

            BottomBasketRegion = new Rect(
                (int)(frameWidth * 0.35),
                (int)(frameHeight * 0.75),
                (int)(frameWidth * 0.3),
                (int)(frameHeight * 0.20)
            );

            // Define court bounds
            CourtBounds = new Rect(
                (int)(frameWidth * 0.1),
                (int)(frameHeight * 0.1),
                (int)(frameWidth * 0.8),
                (int)(frameHeight * 0.8)
            );

            Console.WriteLine($"🏀 Court initialized: {frameWidth}x{frameHeight}");
        }
    }

    public class PerformanceMonitor
    {
        private readonly Stopwatch stopwatch;
        private long frameCount;
        private long totalProcessingTime;

        public PerformanceMonitor()
        {
            stopwatch = new Stopwatch();
        }

        public void RecordFrame()
        {
            frameCount++;
        }

        public double GetFPS()
        {
            return frameCount / stopwatch.Elapsed.TotalSeconds;
        }

        public long GetMemoryUsage()
        {
            return GC.GetTotalMemory(false) / (1024 * 1024);
        }
    }

    // Data classes
    public class VideoInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double Fps { get; set; }
        public int TotalFrames { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class FrameData
    {
        public Mat Frame { get; set; }
        public int FrameNumber { get; set; }
        public TimeSpan GameTime { get; set; }
    }

    public class BallPosition
    {
        public Point2f Position { get; set; }
        public Point2f Velocity { get; set; }
        public TimeSpan GameTime { get; set; }
        public double Confidence { get; set; }
        public bool IsPredicted { get; set; }
    }

    public class PlayerData
    {
        public int PlayerId { get; set; }
        public string Team { get; set; }
        public OpenCvSharp.Point Position { get; set; }
        public Point2f Velocity { get; set; }
        public TimeSpan GameTime { get; set; }
        public OpenCvSharp.Rect BoundingBox { get; set; }
    }

    public class ScoringEvent
    {
        public string Team { get; set; }
        public int Points { get; set; }
        public TimeSpan GameTime { get; set; }
        public string Basket { get; set; }
        public double Confidence { get; set; }
    }

    public class GameResult
    {
        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }
        public int TotalBaskets { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public int TotalFramesProcessed { get; set; }
        public double ProcessingFPS { get; set; }
        public double ScoringPace { get; set; }
        public double TeamAPossession { get; set; }
        public double TeamBPossession { get; set; }
        public double ConfidenceScore { get; set; }
        public List<KeyMoment> KeyMoments { get; set; } = new List<KeyMoment>();
        public List<BallPosition> BallTrajectory { get; set; } = new List<BallPosition>();
        public Dictionary<int, List<PlayerData>> PlayerTracking { get; set; } = new Dictionary<int, List<PlayerData>>();
    }

    public class KeyMoment
    {
        public TimeSpan GameTime { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

    // Helper classes
    public class DetectedPlayer
    {
        public OpenCvSharp.Rect BoundingBox { get; set; }
        public string Team { get; set; }
        public OpenCvSharp.Point Center { get; set; }
    }

    public class TrackedPlayer
    {
        public int Id { get; set; }
        public string Team { get; set; }
        public Point CurrentPosition { get; set; }
        public Point2f Velocity { get; set; }
        public Rect BoundingBox { get; set; }
        public TimeSpan LastSeen { get; set; }
        public bool IsActive { get; set; }

        public void Update(Point newPosition, Rect boundingBox, string team, TimeSpan gameTime)
        {
            // Calculate velocity
            var timeDelta = (gameTime - LastSeen).TotalSeconds;
            if (timeDelta > 0)
            {
                Velocity = new Point2f(
                    (float)((newPosition.X - CurrentPosition.X) / timeDelta),
                    (float)((newPosition.Y - CurrentPosition.Y) / timeDelta)
                );
            }

            CurrentPosition = newPosition;
            BoundingBox = boundingBox;
            Team = team;
            LastSeen = gameTime;
            IsActive = true;
        }
    }

    public class ScoringCandidate
    {
        public string? Team { get; set; }
        public TimeSpan GameTime { get; set; }
        public string? Basket { get; set; }
        public double? Confidence { get; set; }
    }
}