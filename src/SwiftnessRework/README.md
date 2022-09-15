## Swiftness Rework


Changes how **Swiftness** keyword works.  
Predicated on the idea that currently Swiftness does two very strong things a) ignores overload b) doesn't advance enemies' actions counts.  
This mod separates Swiftness into 2 keywords: **Effortless** which ignores overload and **Quick** which doesn't advance action count.  
Effortless + Quick = regular Swiftness. Some skills have one or the other or both. Some Lucy and draw skills got Quick too. Changes can be seen in encyclopedia.  

Technically, this plugin alters Swiftness keyword so it doesn't skip action advance chaging it into Effortless keyword. And Quick is a new keyword entirely.  
As this is a system change plugin other mods can rely on it to add Quick as to cards/relics/items as they see fit. This can be more or less achieved by using function provided in *QuickManager.cs*

---
[*Manual install instructions*](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins#installation)
