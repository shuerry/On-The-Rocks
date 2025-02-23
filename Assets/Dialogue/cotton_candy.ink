// cotton candy
-> cotton_candy

=== cotton_candy ===
- "Soft. Sweet. Classic." #speaker:Rocky
- "Can’t go wrong with cotton candy. But is it… too safe? Too generic?” #speaker:Rocky
- "I don’t want to look like I panicked and just grabbed the first thing I saw." #speaker:Rocky
- "But then again… sharing food is kind of a thing, right?" #speaker:Rocky

// choice
+ ["Take the Cotton Candy"]
    -> handle_pick_up
+ ["Keep Looking"]
    -> END

=== handle_pick_up ===
- "Obtained Cotton Candy!"
-> END