# Quinn Signer: Space Ace

[![Created By: Kellen Madigan](https://img.shields.io/badge/Created%20By:-Kellen%20Madigan-6f42c1)](#ownership--contact)
[![Status](https://img.shields.io/badge/Status:-In%20Development-brightgreen)](#ownership--contact)
[![Program](https://img.shields.io/badge/Georgia%20Tech%20VIP-Sign%20Lab%20Games-gold)](#technology--research-basis)

## Purpose & Importance
This game is being developed as part of the Georgia Tech VIP program with the goal of strengthening communication between Deaf children and their hearing-abled parents.

Research shows that **95% of Deaf children are born to hearing parents** who often have little to no proficiency in sign language. This lack of early access to language can lead to **delays in working memory, social isolation, higher unemployment, and increased risks of mental health struggles** later in life.

By teaching both children and parents sign language in an engaging and playful way, this game aims to:
- Improve parent-child communication
- Strengthen family bonds
- Provide Deaf children with a more positive perspective on their future
- Empower parents to actively participate in their child’s language development

---

## Game Mechanics
The game is structured as an **infinite runner** on-rails experience. Players control a ship navigating through an outer-space environment. When words appear in association with enemy ships:
- **Game time slows down** to allow the player to sign a response.
- The user activates their camera and performs the correct sign.
- If the sign is correct:
  - The recognized word appears in **bright green** in the camera view.
  - The enemy ship explodes.
- If the sign is incorrect:
  - The recognized word appears in **bright red**.
  - The enemy ship remains, continuing the challenge.

Additional mechanics include:
- **Player Destruction**: If the ship collides with an enemy, the player is destroyed and directed to a **try again screen** with a countdown.
- **Retry System**: From the try again screen, the user has **10 seconds to continue** by clicking the button; otherwise, the game returns to the main menu.
- **Main Menu Controls**:
  - Idle background: the ship performs loops and tricks endlessly until the user selects an option.
  - A **Quit button** allows the player to easily exit the application.

---

## Demo Videos
- **Main Menu Idle Screen**

  *Shows the user’s ship looping and performing tricks until a menu button is clicked.*
  
  ![Menu_Idle](https://github.com/user-attachments/assets/9b356bdc-7720-4ab8-8a87-a538c8f6bc1f)


- **Signing Mechanic**

  *Demonstrates the camera-based sign inference system. Correct signs show the word in green and destroy enemies; incorrect signs show the word in red with no enemy destruction.*
  
  ![Gameplay-Sign_Correct-And-Sign_Incorrect](https://github.com/user-attachments/assets/8123a58a-fdb1-4fb5-9a87-13e7f430ed5a)


- **Player Destruction**

  *Shows the player’s ship being destroyed on contact with an enemy and the transition to the try again countdown screen.*
  
  ![Player_Destroyed-Try_Again](https://github.com/user-attachments/assets/40eeb22e-7089-4c79-8740-79e45931b2b9)


- **Try Again Screen & Menu Options**

  *Demonstrates the 10-second continue option, returning to the main menu, and quitting the application.*
  
  ![Quit_Game](https://github.com/user-attachments/assets/ca4d866d-5b03-4574-86cb-5fdd9e4da467)

- **Link to full fist demo video if interested**

  *Use to ctrl/cmd + click to open the video in a new tab...*

  [![Watch the full demo](https://img.youtube.com/vi/IOjGvHunI_0/0.jpg)](https://www.youtube.com/watch?v=IOjGvHunI_0)

---

## Technology & Research Basis
This project builds on **PopSign** and **PopSignAI**, developed by the Georgia Tech VIP program by the name of **PopSign**.

- **PopSign**: A smartphone bubble-shooter game that focused on receptive ASL learning (recognizing signs).
- **PopSignAI**: An advanced version that integrated **Sign Language Recognition (SLR)** to enable *expressive practice*—requiring users to *perform signs themselves*.

My game continues this trajectory by incorporating the **SLR-GTK (Sign Language Recognizer – Gesture Toolkit)**, enabling real-time sign inference during gameplay.

---

## Insights from Parents
The interviews conducted by the **PopSign** VIP team with hearing parents of Deaf children revealed key challenges that shaped this game’s structure:
- **Difficulty attending classes** due to cost, distance, or schedule conflicts.
- **Desire for tools that fit daily life** — parents reported struggling to maintain routines with existing books or videos and wanted short, flexible learning opportunities.
- **Need for expressive practice** - parents felt embarrassed or anxious when trying to sign with native users, and wished for private, low-pressure tools to practice.
- **Motivation to connect with their children** - several parents described moments when spoken language was not enough (e.g., a broken hearing aid, or a child refusing to use a cochlear implant), and ASL became essential for comfort and communication.

These insights inspired design choices like:
- Short, replayable sessions (infinite runner format)
- Immediate feedback (correct/incorrect signs displayed in green/red)
- Rewards & badges to encourage consistent practice
- Future mobile accessibility to allow learning *anytime, anywhere*

---

## Future of the Game
My vision for the future is to bring this game to **mobile devices**, ensuring accessibility and convenience for both children and their parents. By being available on phones and tablets:
- Families can practice anytime, anywhere.
- The mobile platform provides intuitive interaction for younger players.
- Regular updates and new fun addicting ways to learn Sign can be easily dispatched.

The future roadmap includes:
- **Badges & Rewards** for progress and mastery of signs.
- **High Score System & Leaderboards** to encourage replayability and motivate players.
- **Continuous Content Updates** to expand vocabulary and challenges.
- *And More...*

---

## Contributors & Credits

| **Kellen Madigan** | Project Lead / Owner | [@d34db0l7k](https://github.com/d34db0l7k) • kmadigan3@gatech.edu |

| **NAME HERE** | PUT ROLE HERE | [@git_handle](https://github.com/git_handle) • EMAIL |

**Acknowledgements:** Georgia Tech VIP – Sign Lab Games; PopSign & PopSignAI teams
