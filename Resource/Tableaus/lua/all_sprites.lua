local sprites = {}

for _, angle in pairs({ 0, math.pi / 2, math.pi, math.pi / 2 + math.pi }) do
    for _, flipX in pairs({ false, true }) do
        for _, flipY in pairs({ false, true }) do
            for _, sheet in ipairs(ipsum:allSheets()) do
                for i = 0, ipsum:framesInSheet(sheet) - 1 do
                    table.insert(sprites, ipsum:sprite(sheet, i, angle, flipX, flipY))
                end
            end
        end
    end
end

return sprites
