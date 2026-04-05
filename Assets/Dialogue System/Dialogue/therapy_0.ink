// start game
-> therapy_0

=== therapy_0 ===
- "Welcome back, Peggy and Rocky. I’m so glad you both decided to come in today." #speaker:Therapist #pigeon:PeggyP1 #rat:RockyP1
- "Is there anything you want to start with?"
- "..." #speaker:Rocky
- "..." #speaker:Peggy
- "That's alright. What about a fond memory?" #speaker:Therapist
- "Like how Peggy reorganized the chairs last week?" #speaker:Rocky
- "No, like our first date." #speaker:Peggy
- "Classic city romance: boy meets girl, girl dazzles boy, boy sweeps girl off her claws with fried breadcrumbs and cheap carnival games." #pigeon:PeggyP9 #va:Therapy_0_Peggy_0
- "Cheap carnival games??? I’ll have you know I won you that plush donut. Took me three tries and, honestly, half my dinner money… took me as long as that scavenger hunt." #speaker:Rocky #pigeon:PeggyP1 #rat:RockyP2 #va:Therapy_0_Rocky_0
- "Yeah, yeah, I guess you were kinda charming, huh?" #speaker:Peggy #pigeon:PeggyP3 #va:Therapy_0_Peggy_1
- "Both the small and big moments are important." #speaker:Therapist
- "It sounds like that first date had a lot of playful moments... those little gestures that build connection. And I heard there was a scavenger hunt involved?" #speaker:Therapist #pigeon:PeggyP1 #rat:RockyP1
- "Oh, yeah. I challenged him to find the perfect gift for me. Gave him ten minutes to prove his worth." #speaker:Peggy #pigeon:PeggyP4 #va:Therapy_0_Peggy_2
- "No pressure or anything, right?" #speaker:Rocky #rat:RockyP3 #va:Therapy_0_Rocky_1
- "That sounds like a fun challenge." #speaker:Therapist #pigeon:PeggyP1 #rat:RockyP1
// start choices
+ ["Rocky, when you were picking out gifts, what was on your mind?"]
	-> carnival_scene
+ ["Rocky, how did you feel being put on the spot?"]
	-> carnival_scene
+ ["Rocky, did you think Peggy's idea was creative?"]
	-> carnival_scene

=== carnival_scene ===
- "Carnival Scene"
-> END