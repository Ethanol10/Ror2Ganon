# Ganondorf, The King of Evil

Ganondorf, The King of Evil has arrived to wreak havoc! Build up your power by hitting and killing enemies and unleashing it with Warlock Punch!

Network Compatible (unless otherwise found not to be, please tell me if there are some problems when using this mod online.)

Standalone Ancient Scepter Compatible.

Created by Ethanol 10. For any issues or bug reports, contact me on the RoR2 Modding discord, or to me directly: @Ethanol 10#6656

<img src="https://cdn.discordapp.com/attachments/399901440023330816/918474832725823568/unknown.png">
<br><b>Now with fancier effects...</b>
<img src="https://cdn.discordapp.com/attachments/399901440023330816/920678269035106344/unknown.png">
  

## Skills

### Passive: Triforce of Power
#### <img src="https://cdn.discordapp.com/attachments/399901440023330816/918085033204514816/fullTriforceIcon.png">
Every successful hit and kill will build up stacks that increase Ganondorf's armor, up to a maximum of 40 extra armor and 35 extra base damage. Stacks decay after 2 seconds of no build-up. All stacks will be consumed to power up a successful hit of Warlock punch, increasing the damage up to 1250% at the maximum amount of stacks.

### Primary: Punch
<img src="https://cdn.discordapp.com/attachments/399901440023330816/918083335216062464/punchIcon.png"><br>
On the ground, punch forward for 150% damage.
<br>Stunning. When sprinting, dash forward, plowing through enemies for 250% damage. 
<br>When airborne, kick twice for 50% damage and 200% damage.

### Secondary: Wizard's Foot
<img src="https://cdn.discordapp.com/attachments/399901440023330816/920686118024929350/wizardFootIcon.png"><br>
Heavy. Launch yourself forward pushing light enemies away for 150% damage.

### Utility: Flame Choke
<img src="https://cdn.discordapp.com/attachments/399901440023330816/918083336092667904/flameChokeIcon.png"><br>
Dash forward grabbing enemies in a row and choke slamming them dealing 450% damage.

### Special: Warlock Punch
<img src="https://cdn.discordapp.com/attachments/399901440023330816/918083335824211998/warlockPunchIcon.png"><br>
Charge up a powerful punch unleashing 800% damage onto close range foes.

## And More... (well not that much more, but more...) (Unlock them to find out!)

## Changelog
- 2.0.0 -> New Features! Hooray! (This will probably be my last update, for this mod regarding new content (note: for a long time, not forever), as I am a terrible animator. However, if someone wants to offer help with animations for more moves, OR I manage to make my own moves in the time it takes to find someone new, I'm down to continue this project. If the mod breaks with new RoR2 Content, I'll try update it so that it won't be deprecated. Might do some balance changes as well.)
    - 2 New moves! (Unlock them and find out what they are)
    - Another skin! (also with an unlock condition)
    - (slight) VFX updates!
    - Actual hit pauses! (get that weak no-hitpause garbage outta here)
    - Changes to Sound level, sorry for blasting ears in 1.0.3
    - Bug Fixes included (as usual)
        - Fixed Ganon not triggering an attack when he lands with Flame Choke. (For later reference, set the position of the blast attack RIGHT BEFORE you trigger the attack.)
        - Fixed Ganon having ridiculously large item displays when in use with other mods (Unless someone decides to use the name positions that I've made.)
        - Fixed Ganon leaving Greater Wisp Corpses lying all over the place. Gotta make the place clean while destroying everything.
        - Fixed Ganon animation transitions between moves, particularly, Aerial Kick (Punch) and Warlock Punch.
        - Fixed Ganon being NOT immune to executes. 
        - #### FIXED THE STUPID ITEM DISPLAYS YEAAAAAAAAH BOIIIIIII
            - ahem: The item displays should now look not jank *on the head* when transitioning between stages. For those developing their own character and run into the same problem: make sure the child you're attaching it on does not have a collider.
    - Aaaaaaaaaaand balance changes:
        - Warlock Punch was an end goal to the stacks, but nothing more than that. It was also stupidly powerful. I have changed it such that the base damage for Warlock Punch is much lower, while still being a good source of damage when at full stacks. (1650% -> 400% base damage)
        - Specials now don't consume your entire stack of buffs on use. Instead, stacks will only be consumed if you have 50 or more stacks. Between 50 and 100, you will consume 50 stacks to boost the damage of your Special by 416% damage. At 100 stacks+, you will consume 100 stacks to boost the damage of your Special by 1250% damage. If you have less than 50 stacks, you get no bonus damage, but gain 10 stacks on hit.
        - You can now mess around with what Utility/Secondary you'd like! Wanna go on a Kicking spree? Just bind both Utility and Secondary to Wizard's foot! Grab hungry? Bind both to Flame Choke! (Don't worry, defaults are still one of each so you don't have to manually select variety.)
        - Secondary and Utility now share moves, but have varying stats based on what slot they are placed in. Secondary is focused on multiple, short charges and lower damage, but Utility is focused on High damage, consumes Triforce buff and powers your move to 2x more damage depending on whether you have enough buff to use. Without the buff, the move does more damage than secondary. (15 stacks are consumed on a successful hit of a Utility move.)
        - Health growth has been reduced (33 per level -> 15 per level)
        - Armor growth has been increased (0.01 per level -> 0.1 per level)
        - Max base health is reduced (300 -> 250)
        - Stacks of Triforce of Power will now buff your armor more at full stacks (30 -> 40).
        - Stacks of Triforce of Power will now buff your damage (ever so slightly.) (0 -> 35)
        - Punch Damage has been reduced. (200% -> 150%)
        - Punch Launch Force has been reduced. (4000 -> 500)
        - Punch can be fired faster sequentially, end time reduced. (0.6 -> 0.5)
        - Punch hitbox duration has been decreased (0.3s -> 0.2s)
        - Dash Attack (Punch) damage has been reduced (400% -> 250%)
        - Heavy Aerial Kick (Punch) damage has been reduced (300% -> 200%)
        - Flame Choke (Utility) damage has been reduced (500% -> 450%)
        - Wizard's Foot (Secondary) damage has been reduced (250% -> 150%)
        - Wizard's Foot (Secondary) Heavy Damage scaling reduced. (this move was disgusting, it had to be nerfed.)
- 1.0.3 - Fixes to the following: (I promise this is the last thing I'll update for awhile, I'll get back to making something new)
    - Fixed Ganondorf from not being able to grab those stupid hitscan wisps. Slam those guys into the ground for me will ya.
    - Ganon's character model should be *ｐｒｉｓｔｉｎｅ* and not bugged to hell and back anymore.
    - e.g Old:
    <br>
    <img src="https://media.discordapp.net/attachments/918224131080724491/918268889610203186/unknown.png" width=20% height=20%>
    - New:
    <br>
    <img src="https://media.discordapp.net/attachments/399901440023330816/918452322751901706/unknown.png" width=20% height=20%>
- 1.0.2 - Fixes to the following bugs:
    - Changed emission level once again, because I forgot to turn on bloom to test how blinding the light was.
- 1.0.1 - Fixes to the following bugs:
    - Mithrix now spawns instead of just sitting in his cocoon laughing at you for wasting 20 minutes of your run.
    - Ganondorf doesn't get pushed back during Flame Choke now, go grab an entire legion of lemurians!
    - Changed emission level on Ganondorf's mastery skin, hopefully it doesn't blind people now. 
    - Wizard's Foot should now have a lower interrupt priority, so you can try cancelling the kick to do some combos! 
    - Audio across the board has been reduced, sorry for bursting everyone's ears when he does a super powered Warlock Punch.
    - Fixed the Lunar items not being registered for the 8 Lunar Item achievement, hopefully you can get your skin now.
    - Changed the Warlock Punch to only have a random chance to deal a high amount of damage 0.5% of the time.
- 1.0.0 - Initial Release
 
## Future Plans
- Implement new skills
- Fix some bugs (in Known issues)
- Balance the current skills (They may feel a little too strong at the start)  
- Update Ancient Scepter skill for Warlock Punch (it's a little underwhelming)
- Add some on-hit VFX, it completely slipped my mind.

## Known Issues
~~literally none, I am a programming god /sarcasm~~

- Grab moves have some issues when trying to grab a Wandering Vagrant, Greater Wisp, Solus Control Unit (this isn't an exhaustive list), it pushes back against Ganondorf, possibly sending him through the floor and out the level for a bit.
- Special move #2 *very rarely* doesn't register that it hit the floor and as a result, leaves you stuck in the air for a bit. I've only ever encountered this once, so if you do encounter it again, please tell me exactly how you replicated it.
 
## Credits
- Rob's HenryMod -> literally the only thing that convinced me to start making a character, for that thank you so much 
- EnforcerMod -> Code regarding Heat Crash helped me implement Flame Choke.
- Nintendo/Bandai Namco -> Model + Texturing + Animations + Sounds (no this is not officially endorsed by either company)
- TheTimesweeper -> HitboxViewerMod was super helpful in figuring out how big my hitboxes were.
- Lemonlust -> helping out debugging some of the problems during initial release
- My mates, TeaL and Jelly -> for allowing me to basically drag them through my experiements with this mod
- The various people that helped me when I asked stupid questions on the RoR2 Modding discord -> thank you so much for helping me!

