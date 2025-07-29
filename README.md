# TacticsArena - Unity TFT Game

Một game **Đấu Trường Chân Lý (Teamfight Tactics)** đơn giản được xây dựng bằng Unity.

## 🎮 Tính năng chính

### Core Systems
- ✅ **GameManager**: Quản lý trạng thái game (Preparation, Battle, PostBattle)
- ✅ **TurnManager**: Quản lý lượt chơi và timer
- ✅ **Player**: Hệ thống player với health, gold, experience, level
- ✅ **Economy**: Hệ thống kinh tế với lãi suất

### Champion System
- ✅ **ChampionData**: ScriptableObject cho dữ liệu champion
- ✅ **Champion**: Thực thể champion với stats, combat, abilities
- ✅ **Classes & Origins**: Hệ thống phân loại champion (Assassin, Mage, Human, Elf, etc.)

### Battle System
- ✅ **Board**: Bàn cờ hex với placement system
- ✅ **Hex**: Ô hex có thể tương tác
- ✅ **BattleManager**: Quản lý auto-combat
- ✅ **Auto-combat**: Champions tự động tìm target và chiến đấu

### Shop System
- ✅ **ShopManager**: Cửa hàng với probability system
- ✅ **Champion Pool**: Hệ thống random champion theo level
- ✅ **Buy/Refresh**: Mua champion và refresh shop

### Synergy System
- ✅ **SynergyData**: ScriptableObject cho synergy bonuses
- ✅ **SynergyManager**: Tính toán và áp dụng synergy effects
- ✅ **Class/Origin Bonuses**: Bonus stats dựa trên số lượng champion

### UI System
- ✅ **GameUI**: UI hiển thị thông tin player và game state
- ✅ **ChampionShopItem**: UI item cho shop
- ✅ **Events**: Event system cho UI updates

## 🏗️ Kiến trúc

```
TacticsArena/
├── Core/
│   ├── GameManager.cs          # Quản lý trạng thái game
│   ├── Player.cs               # Player data và logic
│   └── TurnManager.cs          # Quản lý lượt chơi
├── Champions/
│   ├── ChampionData.cs         # ScriptableObject cho champion
│   └── Champion.cs             # Combat logic
├── Battle/
│   ├── Board.cs                # Hex board system
│   ├── Hex.cs                  # Individual hex tiles
│   └── BattleManager.cs        # Auto-combat
├── Shop/
│   └── ShopManager.cs          # Shop system
├── Synergies/
│   └── SynergyManager.cs       # Synergy calculations
├── UI/
│   ├── GameUI.cs               # Main game UI
│   └── ChampionShopItem.cs     # Shop item UI
├── TEntity.cs                  # Base entity class
└── MainGameController.cs       # Main controller
```

## 🚀 Cách sử dụng

### 1. Setup Scene
1. Tạo một GameObject trống và add `MainGameController` script
2. Assign các prefabs cần thiết (hexPrefab, championPrefab)
3. MainGameController sẽ tự động tạo và setup các systems

### 2. Tạo Champion Data
1. Right-click trong Project → Create → TacticsArena → Champion Data
2. Điền thông tin champion (name, stats, classes, origins)
3. Assign vào ShopManager's allChampions list

### 3. Tạo Synergy Data
1. Right-click trong Project → Create → TacticsArena → Synergy
2. Định nghĩa synergy bonuses
3. Assign vào SynergyManager's allSynergies list

### 4. Setup UI
1. Tạo Canvas và UI elements
2. Assign vào GameUI script
3. GameUI sẽ tự động update thông tin

## 🎯 Game Flow

1. **Preparation Phase** (30s)
   - Player mua champions từ shop
   - Sắp xếp đội hình trên board
   - Mua experience để level up

2. **Battle Phase** (60s)
   - Auto-combat between player và AI/other players
   - Champions tự động di chuyển và tấn công
   - Abilities được cast khi đủ mana

3. **Post-Battle Phase** (5s)
   - Tính damage dựa trên kết quả battle
   - Cập nhật health và economy
   - Chuyển sang round tiếp theo

## 🔧 Customization

### Thêm Champion mới
```csharp
// Tạo ChampionData mới
var newChampion = ScriptableObject.CreateInstance<ChampionData>();
newChampion.championName = "Fire Mage";
newChampion.classes.Add(ChampionClass.Mage);
newChampion.origins.Add(ChampionOrigin.Human);
newChampion.baseHealth = 120f;
newChampion.baseAttackDamage = 80f;
```

### Thêm Synergy mới
```csharp
// Tạo SynergyData mới
var newSynergy = ScriptableObject.CreateInstance<SynergyData>();
newSynergy.synergyName = "Mage";
newSynergy.championClass = ChampionClass.Mage;
newSynergy.bonuses.Add(new SynergyBonus 
{
    requiredCount = 3,
    bonusDescription = "+50% Spell Damage",
    bonusValue = 0.5f
});
```

### Custom Abilities
Champion abilities có thể được customize trong `Champion.CastAbility()` method:

```csharp
private void CastAbility()
{
    switch (championData.abilityName)
    {
        case "Fireball":
            CastFireball();
            break;
        case "Heal":
            CastHeal();
            break;
        // Add more abilities...
    }
}
```

## 📝 TODO / Cải tiến

- [ ] Item system (equipment cho champions)
- [ ] More champion abilities và effects
- [ ] Carousel round system
- [ ] Multiplayer support
- [ ] Visual effects và animations
- [ ] Sound system
- [ ] Save/Load game state
- [ ] AI opponents
- [ ] Tournament mode

## 🐛 Known Issues

- Champion pathfinding có thể cần cải thiện cho hex grid
- Synergy bonuses chỉ là multiplicative, cần thêm additive bonuses
- UI cần polish và responsive design
- Performance optimization cho large battles

## 📄 License

Dự án này được tạo cho mục đích học tập và phát triển game indie.
