// pretzel
-> pretzel

=== pretzel ===
- "Salty, warm, carby goodness. It’s the perfect snack. Soft on the inside, crispy on the outside…" #speaker:Rocky
// He takes a step closer, already imagining breaking off a piece… then hesitates.
- "Wait. Hold up. Am I just picking things I want?” #speaker:Rocky
// He exhales, rubbing his temple.
- "Think, Rocky. Think. Do you want this because it’s the right choice or because you’re hungry?" #speaker:Rocky

// choice
+ ["Take the Pretzel"]
    -> handle_pick_up
+ ["Keep Looking"]
    -> END

=== handle_pick_up ===
- "Obtained Pretzel!"
-> END