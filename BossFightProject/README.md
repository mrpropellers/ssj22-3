# Project Best Practices
## Coding Tenets
### DO Namespace your code with `namespace BossFight`
This will help us find each other's code in our IDE's and will make refactoring later easier.
### DO Work in your own branch
Name your branch with a prefix unique to you, and a small phrase describing what you're working on, e.g. `devin/pickup-system` -- if you're just working a bunch of things it's fine to use something like `myname/staging` or `myname/dev`
### DO Create a new Unity scene to test your feature
Create a scene for yourself in Scene/Tests and do new work there. Only include the bare minimum objects and components in your scene to validate what you're working on. If you're building/testing a lot of things, consider making your own directory in Tests to store your Scenes.
### DO NOT Modify shared resources unless you are working on integration.
... and check with people first before merging changes back in. Prototype in your own scenes, don't apply prefab overrides until they're ready to be merged into main. If you're working extensively on shared prefabs across multiple scenes, consider working with a Prefab Variant until you're ready to replace the original.
### DO Squash merge back to main 
Merging your whole side branch history will make it hard to revert out breaking commits. Make sure github desktop is set to squash merge, or use `git merge --squash` when merging your branch in. You can also use an interactive rebase (`git fetch && rebase -i origin/main`) to ensure your side branch is up to date and will merge cleanly.
### DO Respect the separation between Runtime and Editor code
If it says `Using UnityEditor` at the top, it should be in an Editor folder. If you absolutely need to do an Editor thing in Runtime code, consider whether you can add a static utility function inside an Editor script instead.
### DO Check for permissively licensed Open Source or free Asset Store projects before starting a new system
Always double-check that you aren't about to write a bunch of code that already exists in a useable format. But, be wary of free Script assets... most of the time they're more of a pain to integrate than they're worth.
### DO Always ask for help when you need it
The point of the Game Jam is to Learn New Things! Check the Resources channel for previously shared resources or ask for help/feedback in the help-and-feedback channel!
