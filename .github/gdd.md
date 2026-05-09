**Bounce Mayhem**

Game Design Document

```
Logo Created by ChatGPT
```
### Team Members

### Mert Mutlu

### Muhammed Mustafa Koyuncu

Document Version: v0.


## Table of the Contents

- 1- About the Game..................................................................................
   - 1.1 - Game Description:.......................................................................
   - 1.2 - Hook:...........................................................................................
- 2 - Place in the Market............................................................................
   - 2.1 - Genre...........................................................................................
   - 2.2 - Similar Games.............................................................................
- 3 - Game Flow (with diagram)................................................................
- 4 - Gameplay...........................................................................................
   - 4.1 - Main Mechanics...........................................................................
   - 4.2 - Power Ups...................................................................................
   - 4.3 - Penalties......................................................................................
- 5 - Level Design......................................................................................
- 6 - User Interface....................................................................................
- 7 - Sound Design....................................................................................
   - 7.1 - Soundtrack...................................................................................
   - 7.1 - Sound Effects (SFX)....................................................................
- 8- Usable Assets.....................................................................................
   - 8.1 - Environment.................................................................................
   - 8.2 - General......................................................................................


## 1- About the Game..................................................................................

### 1.1 - Game Description:.......................................................................

Bounce Mayhem is a 2-player online first person view basketball game where rules, physics,
and the environment can change unpredictably.

### 1.2 - Hook:...........................................................................................

A competitive and chaotic basketball game for 2 players.

## 2 - Place in the Market............................................................................

### 2.1 - Genre...........................................................................................

3D, Sport, Basketball, Casual

### 2.2 - Similar Games.............................................................................

Basketball Simulator

Hoop Fighters: Party Basketball


## 3 - Game Flow (with diagram)................................................................

## 4 - Gameplay...........................................................................................

At the start of the game, players play rock-paper-scissors to see who goes first.
There is only 1 hoop in the game and both players try to score *50 points as fast as possible.
The player who wins the rock-paper-scissors game starts the game by spawning on the
default location and starts the game by throwing the ball. If the ball goes in the basket he
scores 2 points. The next player must pick up the ball before it bounces 3 times on the
ground or he has to shoot with the penalty ball, which only scores 1 point even if he scores.
The player who picks up the ball from the ground loses the ability to move and has to shoot
at the basket from where he picked up the ball. If the ball does not touch the basket, he will
lose 1 point.

### 4.1 - Main Mechanics...........................................................................

```
● Rock Paper Scissors to determine the side that starts first at the start of the game.
● Walking, sprint and jumping.
● Basic basketball mechanics + Basketball 21
○ The more you hold down the ball, the more powerful the launch.
○ Once you have the ball in your hand, you have 5 seconds to shoot.
○ The ball must be picked up by the next player before it bounces 3 times on
the ground.
● Points system
○ 2 points if the ball goes in the basket
○ 1 point if it is scored with a penalty ball
○ -1 point if the ball does not touch the basket.
○ The first player to reach 50 points wins the game.
```

### 4.2 - Power Ups...................................................................................

At certain times in the game, items that give different features will appear on the ground, and
the player who buys them will be given different stats and special powers for the cannon.
● **SpeedUP** - Increases sprint/walk speed.
● **JumpUP** - Increases jumping power.
● **PowerUP** - Increases the throwing power of the ball, allowing it to charge faster..
● **LowGravity** - Reduces the level's gravity for 10 seconds.
● **HighGravity** - Increases the level's gravity for 10 seconds.
● **HeavyBall** - Increases the weight of the ball for 10 seconds.
● **LightBall** - Reduces the weight of the ball for 10 seconds.
● **FireBall** - On his next throw he makes the ball catch fire, if it scores he scores 4
points.
● **Platform Panic** - Spawns a moving wall that stays on the level for 5 seconds.

### 4.3 - Penalties......................................................................................

```
● If the ball does not touch the basket after the player has thrown the ball, the player
loses 1 point.
● If the ball is picked up by the player after bouncing 3 times on the ground, the player
has to shoot with the Penalty Ball and will score only 1 point.
```
## 5 - Level Design......................................................................................

When any player exceeds 25 points, the environment changes, and the game's physics
adapt to suit the new environment.
**Environments:** Moon, Desert, Playground, Forest.



## 6 - User Interface....................................................................................

```
● Main Menu
○ Play (Host Game, Join Game), Settings, Quit
● ESC Menu
○ Resume, Settings, Quit
● Throwing charge bar
```
```
● Point system UI
○ Host player’s point at top left, other player’s point at top right.
○ Blue and Red color.
● Combo + Miss System
○ x1-99 Combo/Miss information at right side.
● Lose & Win Screen
```
## 7 - Sound Design....................................................................................

### 7.1 - Soundtrack...................................................................................

```
● Soundtrack 1: Black Nile
● Soundtrack 2: Cruisin? For A Bluessin?
● Usage: These tracks will play continuously throughout the game, providing an
energetic background during matches. It will help maintain the game's pace and
atmosphere.
```
### 7.1 - Sound Effects (SFX)....................................................................

```
● Basket Scored: A unique sound effect will play every time a player scores a basket,
signaling a successful shot.
● Missed Basket: A different sound effect will play when a basket is missed,
highlighting the failure.
● Ball Bouncing: A sound effect will trigger each time the ball bounces on the ground,
helping to emphasize the ball's interaction with the environment.
● Power-Up Collected: A distinct sound effect will play whenever a player collects a
power-up, signaling the activation of a new ability.
```

## 8- Usable Assets.....................................................................................

### 8.1 - Environment.................................................................................

Low-Poly Simple Nature Pack SimplePoly City - Low Poly Assets

Simple Low Poly Nature Pack CITY package

Low Poly Atmospheric Locations Pack Toon City Pack


### 8.2 - General......................................................................................

Simple Gems and Items Free Sport Balls

Game Input Controller Icons Free Simple UI Elements


