// after minigame
-> carnival_post_minigame

=== carnival_post_minigame ===
- "Okay, okay! I’ll admit it—you’ve got skills." #speaker:Peggy
- "Let’s see what we got!" #speaker:Rocky

// calculate items
+ ["Rat"]
	-> rat_favorites
+ ["Pigeon"]
	-> pigeon_favorites
+ ["Both"]
	-> neutral

=== rat_favorites ===
// Peggy looks at the pile. She forces a smile
- "Oh… wow. You really went all in on the deep-fried theme, huh?" #speaker:Peggy
- "I mean, it’s a solid strategy… just maybe not my thing. But, hey! At least one of us is excited." #speaker:Peggy
// Rocky looks pleased with himself, oblivious to Peggy’s hesitance
// Outcome: Bad (Pigeon Score < Rat Score) 
-> END

=== pigeon_favorites ===
- "No way! A glow stick? A plastic bead necklace? Rocky, this is peak scavenging. It’s got a flair. It’s got style." #speaker:Peggy
- "Oh, I think I’ve seen enough. I’m calling this a win." #speaker:Peggy
// Outcome: Good (Pigeon Score ≥ Rat Score) 
-> END

=== neutral ===
// this one technically shouldn't get picked
- "Hmm… interesting. A little bit of everything. Some sweet, some shiny, some salty. I respect the balance. You weren’t just thinking about yourself." #speaker:Peggy
- "So... that means I win?" #speaker:Rocky
- "Let’s just say… I’m impressed. We’ll call it a tie." #speaker:Peggy
// Outcome: Neutral (Shared Score) 
-> END