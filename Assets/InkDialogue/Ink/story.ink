// start game
-> therapy_0

=== therapy_0 ===
- "Welcome, Peggy and Rocky. I’m so glad you both decided to come in today. Therapy is a big step, and it’s one that can help us explore how you’re feeling and what’s been going on in your relationship. Why don’t we start by sharing a little about how you two met? What brought you together?" #speaker:Therapist
- "Oh, it’s such a story. Classic city romance: boy meets girl, girl dazzles boy, boy sweeps girl off her claws with fried breadcrumbs and cheap carnival games." #speaker:Peggy
- "Cheap carnival games??? I’ll have you know I won you that plush donut. Took me three tries and, honestly, half my dinner money… took me as long as that scavenger hunt" #speaker:Rocky
- "Yeah, yeah, I guess you were kinda charming, huh?" #speaker:Peggy
- "It sounds like that first date had a lot of playful moments..those little gestures that build connection. And I heard there was a scavenger hunt involved?" #speaker:Therapist
- "Oh, yeah. I challenged him to find the perfect gift for me. Gave him ten minutes to prove his worth." #speaker:Peggy
- "No pressure or anything, right?" #speaker:Rocky
// start choices
* "That sounds like a fun challenge. Rocky, when you were picking out gifts, what was on your mind?" #speaker:Therapist
	ABC
	-> carnival_scene
* "What a first impression! Rocky, how did you feel in that situation?" #speaker:Therapist
	DEF
	-> carnival_scene
* "Peggy, that was a really creative idea. Rocky, can you explain what you were thinking?" #speaker:Therapist
	GHI
	-> carnival_scene

=== carnival_scene ===
- [] "Test Carnival"
-> END

=== test_file ===
- hello world
*   ... and I could contain myself no longer.
    'What is the purpose of our journey, Monsieur?'
    'A wager,' he replied.
    * *     'A wager!'[] I returned.
            He nodded. 
            * * *   'But surely that is foolishness!'
            * * *  'A most serious matter then!'
            - - -   He nodded again.
            * * *   'But can we win?'
                    'That is what we will endeavour to find out,' he answered.
            * * *   'A modest wager, I trust?'
                    'Twenty thousand pounds,' he replied, quite flatly.
            * * *   I asked nothing further of him then[.], and after a final, polite cough, he offered nothing more to me. <>
    * *     'Ah[.'],' I replied, uncertain what I thought.
    - -     After that, <>
*   ... but I said nothing[] and <>
- we passed the day in silence.
- -> END