# Void.Zen

Void.Zen is a narrative puzzle game about a presence that shouldn't know you this well.

What starts as breathing exercises and quiet stillness slowly reveals itself to be something else entirely. You'll find yourself breaking into a system that was never meant to be seen — piecing together fragments, chasing signals that reach further than they should.

## Playing on macOS

macOS may block the app on first launch. If you see "cannot be opened because the developer cannot be verified", run this in Terminal:

```bash
xattr -cr MacVoid.Zen.app
```

Then open the app normally.

## Endings

| Ending | Trigger |
|--------|---------|
| **Clean** | Delete the entity via the terminal (`rm -rf ENTITY`) |
| **Purify** | Complete the purification ritual in the void space |
| **Consumed** | Identify the shadow NPC by name — become part of the app |
| **Ending D** | Return after being consumed. Three sessions. No escape. |

## Structure

```
Assets/          — All game scripts, sprites, audio, scenes, and materials
Packages/        — Unity package manifest
ProjectSettings/ — Project configuration
```

## Built With

- Unity 2D (URP)
- TextMeshPro
- Unity Input System
- Unity Services Analytics

## Notes

Some endings write files to your Desktop (`soul_733.txt`, `deleted.lock`). These are part of the game's fiction and can be deleted manually.
