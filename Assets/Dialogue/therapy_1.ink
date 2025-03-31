VAR good_ending = true
// post carnival scene
// The scene fades from the carnival memory. The neon lights, the sounds of the crowd, the warmth of shared laughter...all of it dissolves into the present. The therapist’s office is calm, and neutral
-> therapy_1

=== therapy_1 ===
- "So, when you were picking out gifts, what was on your mind?" #speaker:Therapist
// Rocky hesitates, then exhales as he thinks back to that night.

{ good_ending:
	"I remember being nervous. Like, really nervous. I wanted to get it right. I wanted her to like what I picked." #speaker:Rocky
	// Peggy blinks, caught off guard. She looks at Rocky, studying him like she’s seeing something new.
	"I never knew that." 
	// Rocky lets out a dry laugh, shaking his head.
	"Yeah, well… I probably played it off like I was just goofing around. But I cared. I wanted to do it right." #speaker:Rocky
	"It’s interesting how much effort we put in at the beginning of relationships. The small ways we try to show love. Do you feel like that effort has faded over time?" #speaker:Therapist
	// Rocky hesitates, then nod ... this time, more to himself than anyone else.
- else:
	// Rocky lets out a half-hearted chuckle
	"Honestly? I wasn’t really thinking. I just grabbed what looked good to me." #speaker:Rocky
	"Yeah. I noticed." #speaker:Peggy
	// Rocky opens his mouth but then stops. The way Peggy says it...it’s not mean, but there’s something else there. A quiet disappointment, maybe?
	"It’s easy to make choices based on what feels good in the moment. But relationships aren’t just about individual wants. Were there other times where you made a decision thinking only about yourself?" #speaker:Therapist
	// Rocky hesitates, then nods, slower this time.
}

- "It sounds like that night wasn’t just about the scavenger hunt...it was about how you both showed up for each other. Maybe that’s something to think about. How you bring that effort, that attention, into your relationship now." #speaker:Therapist
// There’s a flicker of something...nostalgia, maybe. Or longing.
// Rocky (nodding slowly)
- "Yeah. I miss that. The laughing, the bantering... I think I forgot how much I loved it." #speaker:Rocky
// Peggy’s gaze softens. She shifts slightly in her chair, and for the first time, she actually looks at him...not just in passing, but really looks at him.
// Peggy (quietly, almost to herself)
- "Me too. Maybe we should try it again sometime." #speaker:Peggy
// (For a moment, neither of them says anything. But something is different. Something unspoken, something unfinished...but something hopeful.)

- "But now, let’s shift gears. Let’s explore a more challenging moment like the argument about your living situation." #speaker:Therapist
// start choices
+ ["Peggy, why don’t you walk us through that memory?"]
	-> subway_scene

=== subway_scene ===
- "Subway Scene"
-> END