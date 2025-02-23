// french fries
-> french_fries

=== french_fries ===
- "Okay, hear me out, who wouldn’t want this? It’s crispy, salty, and melts in your mouth. That’s romance, right?" #speaker:Rocky
// He pauses, scratching his chin.
- "But is this… too basic? Too predictable? Like, 'Oh, of course the rat brings French fries.'” #speaker:Rocky
// His stomach grumbles. He swears it’s louder than the carnival music.
- "Okay, but counterpoint, who complains about French fries? No one. Ever." #speaker:Rocky

// choice
+ ["Take the French Fries"]
    -> handle_pick_up
+ ["Keep Looking"]
    -> END

=== handle_pick_up ===
- "Obtained French Fries!"
-> END