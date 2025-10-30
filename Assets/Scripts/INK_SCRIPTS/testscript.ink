// Global variables
VAR PlayerName = "Io"
VAR HealthPoints = 50

// Unity Functions
//EXTERNAL ChangeMood(characterName, mood)

// STARTS HERE
-> Chapter_0

=== Chapter_0 ===
<size=50>Your</size> <b><color=green>name</color> <i>is</i> {PlayerName}.</b>

#id testscript_ch0_00000
#Command ShowCharacter dulcinea default middle 
#Command PlayAnimationCharacter Dulcinea Wobble 
Once upon a time... 


-> Chapter_1


=== Chapter_1 ===
#id testscript_ch0_00001
#Command ShowCharacter dulcinea happy left
Options here
//{TypingSpeed(2.2)}
* Choice1
 "Picked choice 1. (SPLIT) HP -20."
 ~HealthPoints -= 20 // update variable value
-> Choice1Continue

* Choice2
Picked choice 2
-> Choice2Continue
    
=== Choice1Continue ===
test1
-> Chapter_2

=== Choice2Continue ===
test2 #thought
-> Chapter_2

=== Chapter_2 ===
Chap2 here. Displaying HP: {HealthPoints}.


{ Choice1Continue:
    Picked choice 1
    -> END
}
{ not Choice1Continue: 
    -> NoChoice1
}

=== NoChoice1 ===
did not pick choice 1
-> END
