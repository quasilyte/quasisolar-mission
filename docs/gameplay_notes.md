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

## Flying Through The Galaxy

Traversing from one **star system** to another consumes **fuel**.

There are few ways to recover your fuel:

* Burn **energy resources**: expensive, but can save you some time
* Buy it on a **star base**: relatively inexpensive, but you need to visit a base
* Perform an **idle** action in any system: free, but very slow

![image](https://user-images.githubusercontent.com/6286655/112731247-1b653680-8f47-11eb-91d4-e6d391480e4d.png)

A player can't get stuck in **interstellar space**: you can only peek a destination that is reachable.
It's impossible to wait in interstellar space, neither it is possible to fly somewhere in a middle of nowhere (only systems can be targeted).

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

## Building Star Bases

The player can build new star bases.

Requirements:

* The system should be unoccupied (i.e. it has no other star bases)
* Fleet should have at least 1 Ark vessel

If these requirements are satisfied, "build star base" action will be available.

Since Ark is essentially a ship that transforms into a base, it takes no extra time to build it.

After the base is created, Ark ship leaves the fleet (in other words, it's consumed).

A new base starts with a 1st level and no garrison.

## Battles

![image](https://user-images.githubusercontent.com/6286655/112731615-5c5e4a80-8f49-11eb-8c1f-ca38a423ff3c.png)

Battles are a big part of a game and they shoud be covered in a separate document.
