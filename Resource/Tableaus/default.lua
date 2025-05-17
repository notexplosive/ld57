local elapsedTime = 0

function ipsum.setup()
    ipsum.width = 40
end

local sprite = ipsum.sprite("Entities", 5)

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt
    ipsum.putTile(sprite, 1, 10)
    ipsum.putTile(sprite, 2, 10)
end
