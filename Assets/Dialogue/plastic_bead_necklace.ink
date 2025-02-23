// plastic bead necklace
-> plastic_bead_necklace

=== plastic_bead_necklace ===
- "Huh. Kinda flashy. Kinda fun. Kinda pointless?" #speaker:Rocky
// He tilts his head, watching the beads shimmer. His whiskers twitch.
- "Would she even want something like this? She’s not a kid… but she does kinda have a thing for—” #speaker:Rocky
// He stops himself, suddenly feeling ridiculous.
- "What am I even thinking? This isn’t life or death. It’s a carnival. Just pick something, Rocky." #speaker:Rocky

// choice
+ ["Take the Necklace"]
    -> handle_pick_up
+ ["Keep Looking"]
    -> END

=== handle_pick_up ===
- "Obtained Necklace!"
-> END