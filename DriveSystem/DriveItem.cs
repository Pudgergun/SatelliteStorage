using Terraria;

namespace SatelliteStorage.DriveSystem
{
    public class DriveItem : IDriveItem
    {
        private int _stack;
        private string _stackText = "0";
        public string stackText
        {
            get
            {
                return _stackText;
            }
        }

        public int stack
        {
            get
            {
                return _stack;
            }
            private set
            {
                _stack = value;
                _stackText = Utils.StringUtils.GetStackCount(_stack);
            }
        }

        public int type { get; private set; } = 0;
        public int prefix { get; private set; } = 0;
        public int context { get; private set; } = 0;
        public int recipe { get; private set; } = -1;

        public DriveItem()
        {
            stack = 0;
        }

        public IDriveItem SetStack(int stack)
        {
            this.stack = stack;
            return this;
        }

        public IDriveItem AddStack(int count)
        {
            stack += count;
            return this;
        }

        public IDriveItem SubStack(int count)
        {
            stack -= count;
            return this;
        }

        public IDriveItem SetType(int type)
        {
            this.type = type;
            return this;
        }

        public IDriveItem SetPrefix(int prefix)
        {
            this.prefix = prefix;
            return this;
        }

        public IDriveItem SetContext(int context)
        {
            this.context = context;
            return this;
        }

        public IDriveItem SetRecipe(int recipe)
        {
            this.recipe = recipe;
            return this;
        }

        public static IDriveItem FromItem(Item item)
        {
            return new DriveItem()
                .SetStack(item.stack)
                .SetType(item.type)
                .SetPrefix(item.prefix);
        }

        public Item ToItem()
        {
            Item item = new Item();
            item.type = type;
            item.SetDefaults(item.type);
            item.stack = stack;
            if (item.stack > item.maxStack) item.stack = item.maxStack;
            item.ResetPrefix();
            if (prefix > 0) item.Prefix(prefix);
            item.Refresh(false);
            item.RebuildTooltip();
            return item;
        }
    }
}
