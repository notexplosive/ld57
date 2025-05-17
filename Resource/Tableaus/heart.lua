local elapsedTime = 0


function ipsum.setup()
    ipsum.width = 40
end

local function round(x)
    return x >= 0 and math.floor(x + 0.5) or math.ceil(x - 0.5)
end

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt;
    for i = 0, 3 do
        local t = elapsedTime * 5 + i
        local x = (1 - math.cos(t)) * math.cos(t) * 5 + 20
        local y = (1 - math.cos(t)) * math.sin(t) * 5 + 10
        ipsum.putSpriteTile(x, y, "Utility", 11)
    end
end
