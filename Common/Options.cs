using Domain;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Common
{
    public class Options
    {

        /// <summary>
        /// Contractor Options List Items
        /// </summary>
        /// <param name="userOptionsList"></param>
        /// <returns></returns>
        public static List<SelectListItem> CourtOptionsListItems(List<Court> courtOptionsList)
        {
           
            var selectList = new List<SelectListItem>();
           
            foreach (var element in courtOptionsList)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.CourtId,
                    Text = element.Name
                });
            }

            return selectList;
        }
        /// <summary>
        /// Contractor Options List Items
        /// </summary>
        /// <param name="userOptionsList"></param>
        /// <returns></returns>
        public static List<SelectListItem> PlayerOptionsListItems(List<Profile> profileOptionsList)
        {

            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "0",
                Text = "--Select---"
            });
            foreach (var element in profileOptionsList)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.ProfileId,
                    Text = element.UserName
                });
            }

            return selectList;
        }

        /// <summary>
        /// User Options List Items
        /// </summary>
        /// <param name="userOptionsList"></param>
        /// <returns></returns>
        public static List<SelectListItem> UserOptionsListItems(List<User> userOptionsList)
        {
            var list = userOptionsList.Where(item => item.AccessLevel == "Staff" || item.AccessLevel == "Admin").ToList();


            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "0",
                Text = "--Select---"
            });
            foreach (var element in list)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.UserId,
                    Text = element.FirstName + " " + element.LastName
                });
            }

            return selectList;
        }


        /// <summary>
        /// Profile Options ListItems
        /// </summary>
        /// <param name="profileOptionsList"></param>
        /// <returns></returns>
        public static List<SelectListItem> ProfileOptionsListItems(List<Profile> profileOptionsList)
        {

            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "0",
                Text = "--Select---"
            });
            foreach (var element in profileOptionsList)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.ProfileId,
                    Text =  element.UserName + " " + element.FirstName  + " " + element.LastName + " " + element.PlayerNumber
                });
            }

            return selectList;
        }


 

        /// <summary>
        /// PrivateRun Type Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PrivateRunTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Pick Up 5 v 5 Full Court", Value = "Pick Up 5 v 5 Full Court" });
            selectList.Add(new SelectListItem() { Text = "Pick Up 4 v 4 Half Court", Value = "Pick Up 4 v 4 Half Court" });
            selectList.Add(new SelectListItem() { Text = "Squad 4 v 4 Half Court", Value = "Squad 4 v 4 Half Court" });
            selectList.Add(new SelectListItem() { Text = "Squad 5 v 5 Half Court", Value = "Squad 5 v 5 Half Court" });

            return selectList;
        }

        /// <summary>
        /// PrivateRun Type Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PrivateRunTeamTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Individual", Value = "Individual" });
            selectList.Add(new SelectListItem() { Text = "Team", Value = "Team" });


            return selectList;
        }

        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> AccessLevelOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Standard", Value = "Standard" });
            selectList.Add(new SelectListItem() { Text = "Admin", Value = "Admin" });


            return selectList;
        }
        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PlayerPrivateRunStatusOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Accepted", Value = "Accepted" });
            selectList.Add(new SelectListItem() { Text = "Accepted / Pending", Value = "Accepted / Pending" });
            selectList.Add(new SelectListItem() { Text = "Refund", Value = "Refund" });


            return selectList;
        }

        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ScourtingReportStrengthsOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Scoring Ability", Value = "Scoring Ability: The player has a high ability to score in multiple ways, whether through shooting, driving to the basket, or free-throw proficiency." });
            selectList.Add(new SelectListItem() { Text = "Ball Handling", Value = "Ball Handling: The player shows excellent control of the ball, capable of maintaining possession under pressure and navigating through defenders." });
            selectList.Add(new SelectListItem() { Text = "Basketball IQ", Value = "Basketball IQ: The player displays a deep understanding of the game, including positioning, decision-making, and anticipation of plays." });
            selectList.Add(new SelectListItem() { Text = "Passing Ability", Value = "Passing Ability: The player has exceptional vision and can execute both basic and advanced passes." });
            selectList.Add(new SelectListItem() { Text = "Defensive Ability", Value = "Defensive Ability: The player shows a strong ability to secure both offensive and defensive rebounds." });
            selectList.Add(new SelectListItem() { Text = "Athleticism", Value = "Athleticism: The player exhibits superior physical attributes, including speed, strength, agility, and vertical jump." });
            selectList.Add(new SelectListItem() { Text = "Shooting Range", Value = "Shooting Range: The player has a versatile shooting range, from mid-range jumpers to beyond the three-point arc." });
            selectList.Add(new SelectListItem() { Text = "Leadership", Value = "Leadership: The player has the ability to lead by example and inspire teammates." });
            selectList.Add(new SelectListItem() { Text = "Court Vision", Value = "Court Vision: The player has exceptional awareness of the entire court and can spot opportunities before they develop." });
            selectList.Add(new SelectListItem() { Text = "Finishing at the Rim", Value = "Finishing at the Rim: The player is highly effective in finishing plays near the basket, whether through layups, dunks, or finishing through contact." });
            selectList.Add(new SelectListItem() { Text = "Team Play", Value = "Team Play: The player understands the importance of team dynamics and works well with others." });
            selectList.Add(new SelectListItem() { Text = "Quickness", Value = "Quickness: The player has exceptional speed and the ability to react quickly to changing situations on the floor." });
            selectList.Add(new SelectListItem() { Text = "Transition Play", Value = "Transition Play: The player excels in fast-break situations, either by pushing the ball up the court themselves or running the floor to get easy points." });
            selectList.Add(new SelectListItem() { Text = "High Motor", Value = "High Motor: The player plays with relentless energy, constantly hustling and competing, whether on offense or defense." });
            selectList.Add(new SelectListItem() { Text = "Footwork", Value = "Footwork: The player’s footwork is precise, allowing them to create separation from defenders or close the gap quickly on defense." });
            selectList.Add(new SelectListItem() { Text = "Shot Selection", Value = "Shot Selection: The player demonstrates a good understanding of when and where to take shots." });
            selectList.Add(new SelectListItem() { Text = "Timing", Value = "Timing: The player has excellent timing, whether it’s on defense for blocks and steals, on offense for cutting to the basket, or on rebounds." });
            selectList.Add(new SelectListItem() { Text = "Defensive Awareness", Value = "Defensive Awareness: The player has high awareness on the defensive end, reading the opposing offense well, anticipating plays, and positioning themselves correctly." });
            selectList.Add(new SelectListItem() { Text = "Playmaking", Value = "Playmaking: The player can orchestrate offensive sets, setting up plays, driving to create openings for teammates, and being a primary facilitator." });
            selectList.Add(new SelectListItem() { Text = "Perimeter Defense", Value = "Perimeter Defense: The player is a strong perimeter defender, capable of keeping opposing shooters from getting clean looks." });
            selectList.Add(new SelectListItem() { Text = "Post Moves", Value = "Post Moves: The player is effective in the post, using a combination of footwork, body positioning, and moves." });
            selectList.Add(new SelectListItem() { Text = "Defensive Rebounding", Value = "Defensive Rebounding: The player is excellent at securing defensive rebounds, preventing second-chance opportunities for the opponent." });
            selectList.Add(new SelectListItem() { Text = "Offensive Rebounding", Value = "Offensive Rebounding: The player actively pursues offensive rebounds, giving their team extra possessions." });
            selectList.Add(new SelectListItem() { Text = "Post Defense", Value = "Post Defense: The player effectively defends against post moves, using strength and positioning to prevent easy baskets in the paint." });
            selectList.Add(new SelectListItem() { Text = "Shot Blocking", Value = "Shot Blocking: The player has the ability to disrupt opponents' shots, using timing and athleticism to block attempts at the rim." });
            selectList.Add(new SelectListItem() { Text = "Free Throw Shooting", Value = "Free Throw Shooting: The player is proficient at converting free throws, often capitalizing on fouls to add easy points." });
            selectList.Add(new SelectListItem() { Text = "Toughness", Value = "Toughness: The player plays through physical challenges and remains effective even when facing adversity." });
            selectList.Add(new SelectListItem() { Text = "Offensive Versatility", Value = "Offensive Versatility: The player can score in multiple ways, including shooting, driving, or creating for teammates." });
            selectList.Add(new SelectListItem() { Text = "Defensive Versatility", Value = "Defensive Versatility: The player can guard multiple positions effectively, adjusting to different matchups on the floor." });
            selectList.Add(new SelectListItem() { Text = "Composure Under Pressure", Value = "Composure Under Pressure: The player remains calm and makes smart decisions in high-pressure situations, especially in clutch moments." });
            selectList.Add(new SelectListItem() { Text = "Finishing Through Contact", Value = "Finishing Through Contact: The player excels at finishing plays near the basket despite contact from defenders." });
            selectList.Add(new SelectListItem() { Text = "Transition Defense", Value = "Transition Defense: The player is quick to transition back on defense and effectively guards opponents during fast breaks." });
            selectList.Add(new SelectListItem() { Text = "Pick and Roll Play", Value = "Pick and Roll Play: The player effectively uses the pick and roll, either as the ball handler or as the screener." });
            selectList.Add(new SelectListItem() { Text = "Hustle", Value = "Hustle: The player gives maximum effort on every play, chasing down loose balls, contesting shots, and competing on every possession." });
            selectList.Add(new SelectListItem() { Text = "Spacing", Value = "Spacing: The player understands spacing and positions themselves effectively to create room for teammates to operate." });
            selectList.Add(new SelectListItem() { Text = "Team Defense", Value = "Team Defense: The player understands the importance of team defense, rotating, helping, and communicating with teammates." });
            selectList.Add(new SelectListItem() { Text = "Stamina", Value = "Stamina: The player maintains a high level of energy throughout the game, consistently performing at a high intensity." });
            selectList.Add(new SelectListItem() { Text = "Ball Movement", Value = "Ball Movement: The player excels at moving the ball efficiently, ensuring offensive flow and preventing stagnation." });
            selectList.Add(new SelectListItem() { Text = "Screen Setting", Value = "Screen Setting: The player sets solid screens to free up teammates and create open shots." });
            selectList.Add(new SelectListItem() { Text = "Finishing in Fast Breaks", Value = "Finishing in Fast Breaks: The player is excellent at scoring on fast breaks, finishing quickly and decisively." });
            selectList.Add(new SelectListItem() { Text = "Anticipation", Value = "Anticipation: The player anticipates plays, often jumping passing lanes or being in the right position before the ball arrives." });

            return selectList;
        }


        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ScourtingReportWeaknessOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Poor Ball Handling", Value = "Struggles to control the ball, especially under pressure or against defensive pressure." });
            selectList.Add(new SelectListItem() { Text = "Low Basketball IQ", Value = "Makes poor decisions on the court, such as taking bad shots or missing defensive assignments." });
            selectList.Add(new SelectListItem() { Text = "Inconsistent Shooting", Value = "Shows erratic performance in shooting, unable to maintain accuracy, especially under pressure." });
            selectList.Add(new SelectListItem() { Text = "Weak Finishing at the Rim", Value = "Has difficulty finishing close to the basket, often missing layups or getting blocked easily." });
            selectList.Add(new SelectListItem() { Text = "Limited Lateral Quickness", Value = "Struggles to move quickly side-to-side, which affects defensive positioning and reaction time." });
            selectList.Add(new SelectListItem() { Text = "Poor Free-Throw Shooting", Value = "Struggles to convert free throws, especially in clutch moments or under pressure." });
            selectList.Add(new SelectListItem() { Text = "Lack of Defensive Awareness", Value = "Fails to recognize offensive plays, leaving gaps or failing to switch when needed." });
            selectList.Add(new SelectListItem() { Text = "Overdribbling", Value = "Tends to hold onto the ball too long, leading to turnovers or missed opportunities for teammates." });
            selectList.Add(new SelectListItem() { Text = "Lack of Confidence", Value = "Hesitates when shooting or making decisions, lacking belief in their abilities during key moments." });
            selectList.Add(new SelectListItem() { Text = "Weak Rebounding", Value = "Fails to secure rebounds, especially on the defensive end, leading to second-chance opportunities for the opponent." });
            selectList.Add(new SelectListItem() { Text = "Poor Conditioning", Value = "Struggles with stamina and energy, often tiring quickly and losing effectiveness in later parts of the game." });
            selectList.Add(new SelectListItem() { Text = "Slow First Step", Value = "Struggles to explode quickly off the dribble, making it difficult to drive past defenders." });
            selectList.Add(new SelectListItem() { Text = "Inability to Create Space", Value = "Lacks the footwork or awareness to create open looks for themselves or teammates." });
            selectList.Add(new SelectListItem() { Text = "Ineffective On-Ball Defense", Value = "Struggles to stay in front of their man, often getting beat off the dribble." });
            selectList.Add(new SelectListItem() { Text = "Overly Aggressive", Value = "Makes unnecessary fouls or takes risks that result in turnovers or poor decisions." });
            selectList.Add(new SelectListItem() { Text = "Low Vertical Leap", Value = "Limited jumping ability, which affects their ability to finish plays or contest shots." });
            selectList.Add(new SelectListItem() { Text = "Indecisiveness", Value = "Hesitates or overthinks when making plays, leading to missed opportunities or mistakes." });
            selectList.Add(new SelectListItem() { Text = "Lack of Off-Ball Movement", Value = "Stands still or doesn’t make smart cuts without the ball, limiting offensive flow and spacing." });
            selectList.Add(new SelectListItem() { Text = "Poor Passing", Value = "Has difficulty making accurate passes, resulting in turnovers or poor offensive execution." });
            selectList.Add(new SelectListItem() { Text = "Weak Post Defense", Value = "Struggles to defend players in the post, often getting overpowered or out of position." });
            selectList.Add(new SelectListItem() { Text = "Inconsistent Footwork", Value = "Poor footwork leads to being out of position on defense and inefficient movement on offense." });
            selectList.Add(new SelectListItem() { Text = "Vulnerable to Pressure", Value = "Struggles when faced with defensive pressure, often turning the ball over or rushing decisions." });
            selectList.Add(new SelectListItem() { Text = "Easily Distracted", Value = "Loses focus during key moments, making careless mistakes or getting caught ball-watching." });
            selectList.Add(new SelectListItem() { Text = "Lack of Toughness", Value = "Doesn’t fight through contact or physical play, leading to missed opportunities and weak performances." });
            selectList.Add(new SelectListItem() { Text = "Lack of Court Awareness", Value = "Doesn’t recognize the flow of the game, leading to poor positioning or unnecessary turnovers." });
            selectList.Add(new SelectListItem() { Text = "Not a Threat from Beyond the Arc", Value = "Limited or non-existent three-point shooting ability, which makes them easier to defend." });
            selectList.Add(new SelectListItem() { Text = "Poor Hand-Eye Coordination", Value = "Struggles to catch or control passes, often fumbling or mishandling the ball." });
            selectList.Add(new SelectListItem() { Text = "Slow Reaction Time", Value = "Takes longer to respond to offensive or defensive plays, allowing opponents to exploit weaknesses." });
            selectList.Add(new SelectListItem() { Text = "Lack of Leadership", Value = "Fails to motivate or lead the team on the court, lacking vocal communication or influence." });
            selectList.Add(new SelectListItem() { Text = "Inconsistent Defensive Effort", Value = "Shows lapses in defensive intensity, occasionally letting players slip by or missing rotations." });
            selectList.Add(new SelectListItem() { Text = "Poor Decision-Making Under Pressure", Value = "Makes bad plays when the game is on the line, often forcing shots or turning the ball over." });
            selectList.Add(new SelectListItem() { Text = "Limited Offensive Skills", Value = "Has few scoring options, relying mainly on one move or play, making them predictable." });
            selectList.Add(new SelectListItem() { Text = "Undisciplined Fouling", Value = "Picks up unnecessary fouls, often putting the team in difficult situations or fouling out." });
            selectList.Add(new SelectListItem() { Text = "Lack of Offensive Variety", Value = "Has a limited offensive game, making it easier for defenders to predict and stop their plays." });
            selectList.Add(new SelectListItem() { Text = "Struggles with Fast Breaks", Value = "Lacks speed or awareness in transition, often slowing down the offense or missing scoring opportunities." });
            selectList.Add(new SelectListItem() { Text = "Weak Mental Toughness", Value = "Struggles to stay focused or positive during adversity, often giving up or making mental errors." });
            selectList.Add(new SelectListItem() { Text = "Poor Help Defense", Value = "Doesn’t effectively assist teammates on defense, leaving open lanes or missed rotations." });
            selectList.Add(new SelectListItem() { Text = "Limited Passing Vision", Value = "Has trouble seeing passing lanes, resulting in turnovers or missed assists." });
            selectList.Add(new SelectListItem() { Text = "Stiff Movements", Value = "Lacks fluidity in their movements, making it hard to change directions quickly or maintain balance." });
            selectList.Add(new SelectListItem() { Text = "Limited Range", Value = "Struggles to score from mid-range or long-range, which makes them one-dimensional offensively." });
            selectList.Add(new SelectListItem() { Text = "Inconsistent Defensive Positioning", Value = "Frequently finds themselves out of position, allowing opponents to get easy shots or passes." });
            selectList.Add(new SelectListItem() { Text = "Poor Timing", Value = "Misses opportunities to time their jumps or actions, resulting in missed shots or defensive lapses." });
            selectList.Add(new SelectListItem() { Text = "Over-Reliance on One-Handed Shots", Value = "Prefers using one hand to shoot or handle the ball, which reduces their control and accuracy." });
            selectList.Add(new SelectListItem() { Text = "Not a Threat in Transition", Value = "Lacks the speed or decision-making ability to capitalize on fast break opportunities." });
            selectList.Add(new SelectListItem() { Text = "Low Durability", Value = "Prone to injuries or struggles with maintaining performance over the course of a long game or season." });
            selectList.Add(new SelectListItem() { Text = "Limited Playmaking Ability", Value = "Struggles to create scoring opportunities for teammates, relying mostly on their own offense." });
            selectList.Add(new SelectListItem() { Text = "Ineffective Post Moves", Value = "Struggles to score in the post, lacking advanced footwork or moves to counter defenders." });
            selectList.Add(new SelectListItem() { Text = "One-Dimensional Offense", Value = "Relies too heavily on one offensive skill, making them easy to predict and defend." });
            selectList.Add(new SelectListItem() { Text = "Slow to Adjust", Value = "Struggles to adapt to different styles of play or changes in the flow of the game." });
            selectList.Add(new SelectListItem() { Text = "Over-Commitment on Defense", Value = "Leaves their man open or allows easy passes due to excessive aggression or gambles on defense." });



            return selectList;
        }


        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ReadOrUnReadOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Read", Value = "true" });
            selectList.Add(new SelectListItem() { Text = "UnRead", Value = "false "});


            return selectList;
        }

        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> NotificationTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "CashApp", Value = "CashApp" });
            selectList.Add(new SelectListItem() { Text = "CreditCard", Value = "CreditCard" });
            

            return selectList;
        }

        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> NotificationCategoryOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Payment", Value = "Payment" });
            selectList.Add(new SelectListItem() { Text = "Run", Value = "Run" });
            selectList.Add(new SelectListItem() { Text = "Comment", Value = "Comment" });
            selectList.Add(new SelectListItem() { Text = "Event", Value = "Event" });
            selectList.Add(new SelectListItem() { Text = "Invite", Value = "Invite" });

            return selectList;
        }


        /// <summary>
        /// Access Level Options List Items
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PaymentMethodOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "CashApp", Value = "CashApp" });
            selectList.Add(new SelectListItem() { Text = "Zelle", Value = "Zelle" });
            selectList.Add(new SelectListItem() { Text = "Cash", Value = "Cash" });

            return selectList;
        }


        /// <summary>
        /// Post Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PostOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Info", Value = "Info" });
            selectList.Add(new SelectListItem() { Text = "Product", Value = "Product" });

            return selectList;
        }


        /// <summary>
        /// PostType Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PostTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "News", Value = "News" });
            selectList.Add(new SelectListItem() { Text = "Event", Value = "Event" });
            selectList.Add(new SelectListItem() { Text = "Blog", Value = "Blog" });
            selectList.Add(new SelectListItem() { Text = "User", Value = "User" });

            return selectList;
        }

        /// <summary>
        /// ProductType Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ProductTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Clothing", Value = "Clothing" });
            selectList.Add(new SelectListItem() { Text = "Accsociers", Value = "Accsociers" });
            selectList.Add(new SelectListItem() { Text = "Shoes", Value = "Shoes" });

            return selectList;
        }

        /// <summary>
        /// Status Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> StatusOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Active", Value = "Active" });
            selectList.Add(new SelectListItem() { Text = "InActive", Value = "InActive" });
            selectList.Add(new SelectListItem() { Text = "TBA", Value = "TBA" });
            selectList.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            
            return selectList;
        }

        /// <summary>
        /// PostCategory Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PostCategoryOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Dunks", Value = "Dunks" });
            selectList.Add(new SelectListItem() { Text = "Plays", Value = "Plays" });
            selectList.Add(new SelectListItem() { Text = "Crossovers", Value = "Crossovers" });
            selectList.Add(new SelectListItem() { Text = "Technology", Value = "Technology" });
            selectList.Add(new SelectListItem() { Text = "Etiquette", Value = "Etiquette" });
            selectList.Add(new SelectListItem() { Text = "Players", Value = "Players" });
            selectList.Add(new SelectListItem() { Text = "Other", Value = "Other" });

            return selectList;
        }


        /// <summary>
        /// PrivateRun SkillLevel Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PrivateRunSkillLevelOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Recreational", Value = "Recreational" });
            selectList.Add(new SelectListItem() { Text = "Competitive", Value = "Competitive" });

            return selectList;
        }


        /// <summary>
        /// PrivateRun SkillLevel Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> FinancialCategoryOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Service", Value = "Service" });
            selectList.Add(new SelectListItem() { Text = "Rental", Value = "Rental" });
            selectList.Add(new SelectListItem() { Text = "Payment", Value = "Payment" });

            return selectList;
        }

        /// <summary>
        /// PrivateRun SkillLevel Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> FinancialTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Expense", Value = "Expense" });
            selectList.Add(new SelectListItem() { Text = "Income", Value = "Income" });

            return selectList;
        }

        /// <summary>
        /// PrivateRun SkillLevel Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> FinancialPaymentFrequencyOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem() { Text = "Weekly", Value = "Weekly" });
            selectList.Add(new SelectListItem() { Text = "Monthly", Value = "Monthly" });
            selectList.Add(new SelectListItem() { Text = "Yearly", Value = "Yearly" }); 
            selectList.Add(new SelectListItem() { Text = "One Time Payment", Value = "One Time Payment" });

            return selectList;
        }

        /// <summary>
        /// PrivateRun SkillLevel Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ClothingOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem() { Text = "Sm", Value = "Sm" });
            selectList.Add(new SelectListItem() { Text = "L", Value = "L" });
            selectList.Add(new SelectListItem() { Text = "XL", Value = "XL" });
            selectList.Add(new SelectListItem() { Text = "XXL", Value = "XXL" });

            return selectList;
        }



        /// <summary>
        /// Court Status Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> CourtStatusOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Active", Value = "Active" });
            selectList.Add(new SelectListItem() { Text = "InActive", Value = "InActive" });

            return selectList;
        }

        /// <summary>
        /// On Cancelled Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> OnCancelledOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Active", Value = "Active" });
            selectList.Add(new SelectListItem() { Text = "InActive", Value = "InActive" });
            selectList.Add(new SelectListItem() { Text = "On", Value = "On" });
            selectList.Add(new SelectListItem() { Text = "Cancelled", Value = "Cancelled" });
            selectList.Add(new SelectListItem() { Text = "TDB", Value = "TDB" });

            return selectList;
        }


        /// <summary>
        /// On Cancelled Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> DocumentTypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Invoice", Value = "Invoice" });
            selectList.Add(new SelectListItem() { Text = "Receipt", Value = "Receipt" });
            selectList.Add(new SelectListItem() { Text = "Service", Value = "Service" });


            return selectList;
        }

        /// <summary>
        /// On Cancelled Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> DocumentCategoryOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Expense", Value = "Expense" });
            selectList.Add(new SelectListItem() { Text = "Income", Value = "Income" });

            return selectList;
        }

        /// <summary>
        /// YesNoOptions ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> YesNoOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "Yes", Value = "Yes" });
            selectList.Add(new SelectListItem() { Text = "No", Value = "No" });

            return selectList;
        }

        /// <summary>
        /// HeightOptions ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> HeightOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "5'0", Value = "5'0", });
            selectList.Add(new SelectListItem() { Text = "5'1", Value = "5'1", });
            selectList.Add(new SelectListItem() { Text = "5'2", Value = "5'2", });
            selectList.Add(new SelectListItem() { Text = "5'3", Value = "5'3", });
            selectList.Add(new SelectListItem() { Text = "5'4", Value = "5'4", });
            selectList.Add(new SelectListItem() { Text = "5'5", Value = "5'5", });
            selectList.Add(new SelectListItem() { Text = "5'6", Value = "5'6", });
            selectList.Add(new SelectListItem() { Text = "5'7", Value = "5'7", });
            selectList.Add(new SelectListItem() { Text = "5'8", Value = "5'8", });
            selectList.Add(new SelectListItem() { Text = "5'9", Value = "5'9", });
            selectList.Add(new SelectListItem() { Text = "5'10", Value ="5'10", });
            selectList.Add(new SelectListItem() { Text = "5'11", Value ="5'11", });
            selectList.Add(new SelectListItem() { Text = "6'0", Value = "6'0", });
            selectList.Add(new SelectListItem() { Text = "6'1", Value = "6'1", });
            selectList.Add(new SelectListItem() { Text = "6'2", Value = "6'2", });
            selectList.Add(new SelectListItem() { Text = "6'3", Value = "6'3", });
            selectList.Add(new SelectListItem() { Text = "6'4", Value = "6'4", });
            selectList.Add(new SelectListItem() { Text = "6'5", Value = "6'5", });
            selectList.Add(new SelectListItem() { Text = "6'6", Value = "6'6", });
            selectList.Add(new SelectListItem() { Text = "6'7", Value = "6'7", });
            selectList.Add(new SelectListItem() { Text = "6'8", Value = "6'8", });
            selectList.Add(new SelectListItem() { Text = "6'9", Value = "6'9", });
            selectList.Add(new SelectListItem() { Text = "6'10", Value ="6'10", });
            selectList.Add(new SelectListItem() { Text = "6'11", Value ="6'11", });
            selectList.Add(new SelectListItem() { Text = "7'0", Value = "7'0", });
            selectList.Add(new SelectListItem() { Text = "7'1", Value = "7'1", });
            selectList.Add(new SelectListItem() { Text = "7'2", Value = "7'2", });
            selectList.Add(new SelectListItem() { Text = "7'3", Value = "7'3", });
            selectList.Add(new SelectListItem() { Text = "7'4", Value = "7'4", });
            selectList.Add(new SelectListItem() { Text = "7'5", Value = "7'5", });

            return selectList;
        }

        /// <summary>
        /// Player Archetype Options ListItems
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PlayerArchetypeOptionsListItems()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem() { Text = "PG", Value = "PG" });
            selectList.Add(new SelectListItem() { Text = "SG", Value = "SG" });
            selectList.Add(new SelectListItem() { Text = "SF", Value = "SF" });
            selectList.Add(new SelectListItem() { Text = "PF", Value = "PF" });
            selectList.Add(new SelectListItem() { Text = "C", Value = "C" });
            
            return selectList;
        }

    }
}
