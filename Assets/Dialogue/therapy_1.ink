// post carnival scene
// The scene fades from the carnival memory. The neon lights, the sounds of the crowd, the warmth of shared laughter...all of it dissolves into the present. The therapist’s office is calm, and neutral
-> therapy_1

=== therapy_1 ===
- "So, when you were picking out gifts, what was on your mind?" #speaker:Therapist
// Rocky hesitates, then exhales as he thinks back to that night.

// Player Choice – Rocky’s Reflection:
+ ["I just grabbed what I liked."]
	-> i_grabbed_what_i_liked
+ ["I was really trying to impress her."]
	-> i_was_trying_to_impress_her
+ ["I wasn’t sure what the right choice was."]
	-> i_wasnt_sure_what_the_right_choice_was

=== i_grabbed_what_i_liked ===
// Rocky lets out a half-hearted chuckle
- "Honestly? I wasn’t really thinking. I just grabbed what looked good to me." #speaker:Rocky
- "Yeah. I noticed." #speaker:Peggy
// Rocky opens his mouth but then stops. The way Peggy says it...it’s not mean, but there’s something else there. A quiet disappointment, maybe?
- "It’s easy to make choices based on what feels good in the moment. But relationships aren’t just about individual wants. Were there other times where you made a decision thinking only about yourself?" #speaker:Therapist
// Rocky hesitates, then nods, slower this time.
-> carnival_ending

=== i_was_trying_to_impress_her ===
- "I remember being nervous. Like, really nervous. I wanted to get it right. I wanted her to like what I picked." #speaker:Rocky
// Peggy blinks, caught off guard. She looks at Rocky, studying him like she’s seeing something new.
- "I never knew that." 
// Rocky lets out a dry laugh, shaking his head.
- "Yeah, well… I probably played it off like I was just goofing around. But I cared. I wanted to do it right." #speaker:Rocky
- "It’s interesting how much effort we put in at the beginning of relationships. The small ways we try to show love. Do you feel like that effort has faded over time?" #speaker:Therapist
// Rocky hesitates, then nod ... this time, more to himself than anyone else.
-> carnival_ending

=== i_wasnt_sure_what_the_right_choice_was ===
// Rocky exhales, shaking his head with a small, self-deprecating chuckle.
- "I remember overthinking everything. Like… what if I picked something weird? What if she thought I was trying too hard? Or not enough?" #speaker:Rocky
// Peggy’s expression softens as she listens. She tilts her head slightly, as if recognizing something familiar in his words.
- "I get that." #speaker:Peggy
// Rocky glances at her, surprised.
- "I mean, I do that too. With different things, but… yeah." #speaker:Peggy
// The therapist watches them both, a small knowing smile on their face.
- "Uncertainty is normal. But hesitation can keep us from fully showing up for the people we care about. Do either of you feel like you still hold back?" #speaker:Therapist
// A pause. Then, a quiet, mutual nod from both of them.
-> carnival_ending

=== carnival_ending ===
- "It sounds like that night wasn’t just about the scavenger hunt...it was about how you both showed up for each other. Maybe that’s something to think about. How you bring that effort, that attention, into your relationship now." #speaker:Therapist
// There’s a flicker of something...nostalgia, maybe. Or longing.
// Rocky (nodding slowly)
- "Yeah. I miss that. The laughing, the bantering... I think I forgot how much I loved it." #speaker:Rocky
// Peggy’s gaze softens. She shifts slightly in her chair, and for the first time, she actually looks at him...not just in passing, but really looks at him.
// Peggy (quietly, almost to herself)
- "Me too. Maybe we should try it again sometime." #speaker:Peggy
// (For a moment, neither of them says anything. But something is different. Something unspoken, something unfinished...but something hopeful.)
-> subway_scene

=== subway_scene ===
- "But now, let’s shift gears. Let’s explore a more challenging moment like the argument about your living situation. Peggy, why don’t you walk us through that memory?" #speaker:Therapist
- "Subway Scene"
-> END