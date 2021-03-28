This is not the game manual.

This is something like a single player game mode design document.

## The Game Goal

In short, player mission is simple: collect N artifacts (N may depend on the game difficulty) before
the in-game day X (X depends on the game difficulty). Then player needs to defeat the main boss.

The problem is, the player starts with a weak vessel and with one star base. A galaxy is quite big
and there are a lot of dangers out there. One can't simply fly through it and find all artifacts
without any troubles.

In order to win, the player should:

* Build a powerful fleet
* Create new star bases
* Forge alliances with other races
* Collect distant planet resources
* Win in dozens of great space battles

Losing conditions:

* All Earthling bases are destroyed
* Flagship is destroyed
* N artifacts are not collected before the day X
* The final hostile armada is not defeated

## Flying Through The Galaxy

Traversing from one **star system** to another consumes **fuel**.

There are few ways to recover your fuel:

* Burn **energy resources**: expensive, but can save you some time
* Buy it on a **star base**: relatively inexpensive, but you need to visit a base
* Perform an **idle** action in any system: free, but very slow

![image](https://user-images.githubusercontent.com/6286655/112731247-1b653680-8f47-11eb-91d4-e6d391480e4d.png)

A player can't get stuck in **interstellar space**: you can only peek a destination that is reachable.
It's impossible to wait in interstellar space, neither it is possible to fly somewhere in a middle of nowhere (only systems can be targeted).

## Player Fleet

![image](https://user-images.githubusercontent.com/6286655/112732731-c3c9c980-8f4c-11eb-89e7-41f93e4e5bc8.png)

A fleet is a group of vessels that fly together.

Player can have a fleet of size 6. One flagship (player-controlled) and 5 escort vessels.

Not every vessel has to participate in combat, but flagship is obligated to.

> TODO: or should it be OK to have a bots-vs-bots battle without a player?

The player can choose what vessels are included into the fleet. It can consist of all military crafts
or it can have some freighters to make it easy to carry more resources.

## Planetary Resources

There are 3 basic types of **planetary resources**:

* Minerals. The most common resource.
* Organics. Valuable resource with a good price.
* Energy. Even more valuable, but can also be converted to **fuel**.

Every resource is needed to help a **star base** growth.

As the name suggests, planetary resources can be mined on planets. To do that, **drones** are needed.
Drones can be bought on a star base.

Every star system has 1-3 planets with resourcs. Such planets may have different resources available.

Some notes to keep in mind:

* Planet resources are infinite
* Drone collects some resources each day
* The amount of resources gathered every day vary from planet to planet
* Collected resources must be loaded to a vessel
* There is a resource storage cap that will make the drone stop collecting resources if it's reached

For example, there could be a planet with these properties: minerals-1, organics-3, energy-0.
It means that every day drone will collect 1 mineral and 3 units of organics.

It's up to the player to deploy droids effectively.

## Earthling Star Bases

![image](https://user-images.githubusercontent.com/6286655/112732527-8d3f7f00-8f4b-11eb-81b6-007e1f006d53.png)

Player-controlled bases can be entered.

Every star base has a level.

A star base level affects what you can do on a base and how effectively it operates:

* Max level of equipment it can produce (given that it's researched)
* Vessel production speed
* Research speed

Every base gets some development points every day. When it reaches some threshold,
a new level is granted. This process is slow and player can sped it up by offloading
the resources required by a base.

A base accepts all kinds of resources, but some of them are needed more than others.
Selling the resource that has higher demand will result in a faster base growth.

Base offers these services:

* Equipment store
* Refuelling
* Vessel repairs
* Drone store
* Resources market
* Research
* Vessel production

| Star Base Level | Max Weapon Level |
|---|---|
| 1 | Level 1 normal and special weapons |
| 2 | Level 2 normal weapons |
| 3 | Level 2 normal and special weapons |
| 4 | Level 3 normal weapons |
| 5 | Level 3 normal and special weapons |

| Star Base Level | Max Shield Level |
|---|---|
| 1 | Level 1 |
| 2 | Level 1 |
| 3 | Level 2 |
| 4 | Level 2 |
| 5 | Level 3 |

## Equipment Shop

Theoretically, an equipment shop can contain all game items.

But there are requirements for every item that must be satisfied in order for that item to be available for sale.

Every item has these potential requirements:

* Required research
* Minimal star base level

For example, Krigia `Scythe` weapon requires these researches to be completed:

* `Krigia Weapons I`
* `Scythe`

After that, it will be possible to produce that weapon at any allied star base.

Unique weapons that can be found in space usually require a dedicated research. For example,
when you'll find a `Stormbringer`, there will be a choise: you can either sell it or give it to the
research lab. Researching that item will makes that item instance impossible to use, but
it will be possible to produce them in the future.

## Enemy Star Bases

![image](https://user-images.githubusercontent.com/6286655/112732471-3043c900-8f4b-11eb-96d7-f8e013a053ba.png)

Being in a system that contains a hostile star base is dangerous.

Every day there is a chance that some of the star base fleet ships will attack the player.
The number of attackers depends on the garrison size. If a player uses **attack** action,
that chance is equal to 100%; there will be a battle every day.

If entire garrison is defeated, the base can be destroyed. In order to do that,
player should use **attack** action. That will inflict damage to the base, until it's destroyed.

Note that a base will produce new starships while it's active.

## Researching

Every base can perform a research.

You can order different kinds of researches:

* A study on alien equipment
* Artifact research

Studying alien equipment requires specific vessel debris. When you destroy a vessel of X race, you get some X-specific debris.
You can choose what should be focused: alien weapons or vessel designs.

> Maybe there will be a way to get some technology through a diplomacy, like in Civilization-ish games.

To research an artifact you first need... an artifact. When artifact is researched, it can be used by any vessel. Artifacts can
be found in star systems and in some random encounters.

> TODO: how does one make artifact available for production?

Research tree can be found in a [dedicated document](/docs/research_tree.md).

## Building Star Bases

The player can build new star bases.

Requirements:

* The system should be unoccupied (i.e. it has no other star bases)
* Fleet should have at least 1 **Ark** vessel

If these requirements are satisfied, "build star base" action will be available.

Since Ark is essentially a ship that transforms into a base, it takes no extra time to build it.

After the base is created, Ark ship leaves the fleet (in other words, it's consumed).

A new base starts with a 1st level and no garrison.

## Wandering Units

Player is not the only force in the galaxy that moves from one star system to another.

There are different fleets that may roam the space:

* Unique groups (related to random encounters)
* Star base patrols
* Task force
* Free roam group

Unique group could resolve into a battle or an ally. Most of the time they're bosses that the player can fight to get the reward.

Star base patrols are units that fly between the star systems that are located in the vicinity of that base. These
fleets will attack their enemies, but will not attempt a siege of the other star base.

Task forces will try to besiege enemy star base. They'll either do it, or die. If they succeed, they'll return to the base
from which they originated. If that base is destroyed, they'll try to find the closest allied base; if there are
no allied bases, they'll turn into a free roam group.

A free roam group travels through space without any particular destination. Most of the free roam groups belong to scavengers.

## Scavengers

Scavengers are the local "space pirates". They attack weak bases and fleets in attempt to scavenge their resources.

They don't have own star bases.

> TODO: explain where and when do they spawn.

## Random Events

There are 2 kinds of random events:

* One-off events
* Periodic events

One-off events are determined during the new game generation. They are unique, so the same event won't happen twice in a game.

Periodic events are less unique and the same kind of event can happen several times during a game. They're usually less drastic.

> TODO: figure out which kinds of random events we have and how they're resolved.

## Computer-controlled Pilot Ranks

Every computer-controlled vessel has associated bot rank.

Bot rank affects the battle performance.

Higher rank means:

* Better chances to do a snipe shot (with velocity calculations)
* May avoid asteroids more successfully
* Can perform more advanced movement tricks
* More careful energy management
* Less deliberate mistakes (bots make such mistakes sometimes; experienced bots do it very rarely)
* Higher self-preservation (may retreat more when low on health)
* More skillfull shield usage (experienced bot does not use a shield if projectile would miss anyway)

In a single player mode, all Earthling pilots start as rookies and then gain experience through battles.
This makes it more desirable to keep allied pilots alive, so they can improve.

## Earning Credits

Ways to get credits:

* Sell planetary resources (collected by drones)
* Sell vessel debris (collected by fighting)
* Sell equipment
* Some random events can give you credits

> Note: selling equipment only gives credit gains; it doesn't count as giving an equipment for the research.

## Getting Equipment

Ways to get equipment:

* Buy on a friendly star base: limited by a base technological limitations
* Get from a random event

Note that you don't normally get equipment from battles; all you get is sellable (and researchable) debris.

## Battles

![image](https://user-images.githubusercontent.com/6286655/112731615-5c5e4a80-8f49-11eb-8c1f-ca38a423ff3c.png)

Battles are a big part of a game and they shoud be covered in a separate document.
