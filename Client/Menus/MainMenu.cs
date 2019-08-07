﻿using CitizenFX.Core;
using MenuAPI;
using PocceMod.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocceMod.Client.Menus
{
    public class MainMenu : Menu
    {
        private static readonly int MenuKey;
        private readonly Dictionary<int, Func<Task>> _menuItemActions = new Dictionary<int, Func<Task>>();
        private readonly Dictionary<int, List<Func<Task>>> _menuListItemActions = new Dictionary<int, List<Func<Task>>>();

        static MainMenu()
        {
            MenuKey = Config.GetConfigInt("MenuKey");
            if (MenuKey == 0)
            {
                Common.Notification("No PocceMod menu key configured");
            }

            MenuController.MenuToggleKey = (Control)MenuKey; // Control.SelectCharacterMichael;
            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.DontOpenAnyMenu = true;

            try
            {
                MenuController.MenuAlignment = Config.GetConfigBool("MenuRightAlign") ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            }
            catch (AspectRatioException)
            {
                Common.Notification("Unsupported aspect ratio! PocceMod menu is force left aligned");
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
            }
        }

        public MainMenu() : base("PocceMod", "menu")
        {
            VehicleMenu = new VehicleMenu();
            PropMenu = new PropMenu();
            SkinMenu = new SkinMenu();
            MassScenarioMenu = new MassScenarioMenu();

            MenuController.AddMenu(this);
            MenuController.AddSubmenu(this, VehicleMenu);
            MenuController.AddSubmenu(this, PropMenu);
            MenuController.AddSubmenu(this, SkinMenu);
            MenuController.AddSubmenu(this, MassScenarioMenu);

            OnItemSelect += async (_menu, _item, _index) =>
            {
                if (_menuItemActions.TryGetValue(_index, out Func<Task> action))
                {
                    await action();
                    CloseMenu();
                }
            };

            OnListItemSelect += async (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                if (_menuListItemActions.TryGetValue(_itemIndex, out List<Func<Task>> actions))
                {
                    await actions[_listIndex]();
                    CloseMenu();
                }
            };
        }

        public VehicleMenu VehicleMenu { get; private set; }
        public PropMenu PropMenu { get; private set; }
        public SkinMenu SkinMenu { get; private set; }
        public MassScenarioMenu MassScenarioMenu { get; private set; }

        public static bool IsOpen
        {
            get { return MenuController.IsAnyMenuOpen(); }
        }

        public void AddMenuItemAsync(string item, Func<Task> onSelect)
        {
            if (MenuKey > 0)
                MenuController.DontOpenAnyMenu = false;

            var menuItem = new MenuItem(item);
            AddMenuItem(menuItem);
            _menuItemActions.Add(menuItem.Index, onSelect);
        }

        public void AddMenuItem(string item, Action onSelect)
        {
            AddMenuItemAsync(item, () => { onSelect(); return Task.FromResult(0); });
        }

        public void AddMenuListItemAsync(string item, string subitem, Func<Task> onSelect)
        {
            if (MenuKey > 0)
                MenuController.DontOpenAnyMenu = false;

            foreach (var menuItem in GetMenuItems())
            {
                if (menuItem is MenuListItem && menuItem.Text == item)
                {
                    var subitems = ((MenuListItem)menuItem).ListItems;
                    subitems.Add(subitem);
                    _menuListItemActions[menuItem.Index].Add(onSelect);
                    return;
                }
            }

            var menuListItem = new MenuListItem(item, new List<string> { subitem }, 0);
            AddMenuItem(menuListItem);
            _menuListItemActions.Add(menuListItem.Index, new List<Func<Task>> { onSelect });
        }

        public void AddMenuListItem(string item, string subitem, Action onSelect)
        {
            AddMenuListItemAsync(item, subitem, () => { onSelect(); return Task.FromResult(0); });
        }

    }
}
