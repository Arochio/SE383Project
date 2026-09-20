using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public long Points { get; set; } = 0;

        [BindProperty]
        public int ClickValue { get; set; } = 1;

        [BindProperty]
        public Dictionary<string, UpgradeData> Upgrades { get; set; } = new();

        public void OnGet()
        {
            // Initialize upgrades if not already done
            if (!Upgrades.Any())
            {
                Upgrades = new Dictionary<string, UpgradeData>
                {
                    { "auto-clicker", new UpgradeData { Id = "auto-clicker", Name = "Auto Clicker", Description = "Clicks automatically every second", Cost = 10, PointsPerSecond = 1, Level = 0 } },
                    { "double-click", new UpgradeData { Id = "double-click", Name = "Double Click", Description = "Each click is worth 2 points", Cost = 25, ClickMultiplier = 2, Level = 0 } },
                    { "click-frenzy", new UpgradeData { Id = "click-frenzy", Name = "Click Frenzy", Description = "Each click is worth 5 points", Cost = 100, ClickMultiplier = 5, Level = 0 } },
                    { "mega-farm", new UpgradeData { Id = "mega-farm", Name = "Mega Farm", Description = "Generates 10 points per second", Cost = 500, PointsPerSecond = 10, Level = 0 } }
                };
            }
        }

        public IActionResult OnPostClick()
        {
            Points += ClickValue;
            return Page();
        }

        public IActionResult OnPostBuyUpgrade(string upgradeId)
        {
            if (Upgrades.TryGetValue(upgradeId, out var upgrade))
            {
                if (Points >= upgrade.Cost)
                {
                    Points -= upgrade.Cost;
                    upgrade.Level++;

                    // Update click value based on upgrades
                    if (upgradeId == "double-click")
                    {
                        ClickValue = 2;
                    }
                    else if (upgradeId == "click-frenzy")
                    {
                        ClickValue = 5;
                    }

                    // Update upgrade cost for next level
                    upgrade.Cost = (int)(upgrade.Cost * 1.15);
                }
            }

            return Page();
        }
    }

    public class UpgradeData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Cost { get; set; }
        public int PointsPerSecond { get; set; }
        public int ClickMultiplier { get; set; } = 1;
        public int Level { get; set; }
    }
}
