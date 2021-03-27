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
