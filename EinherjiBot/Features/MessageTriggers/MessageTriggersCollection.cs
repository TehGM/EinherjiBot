using System.Collections;

namespace TehGM.EinherjiBot.MessageTriggers
{
    public class MessageTriggersCollection : ICollection<MessageTrigger>, ICacheableEntity<ulong>
    {
        public ulong GuildID { get; }
        private readonly List<MessageTrigger> _triggers;

        public MessageTriggersCollection(ulong guildID, IEnumerable<MessageTrigger> triggers)
        {
            this.GuildID = guildID;
            this._triggers = new List<MessageTrigger>(triggers);
        }

        public MessageTriggersCollection(ulong guildID, int capacity)
        {
            this.GuildID = guildID;
            this._triggers = new List<MessageTrigger>(capacity);
        }

        public MessageTriggersCollection(ulong guildID)
        {
            this.GuildID = guildID;
            this._triggers = new List<MessageTrigger>();
        }

        public ulong GetCacheKey()
            => this.GuildID;

        #region ICollection implementation
        public int Count => ((ICollection<MessageTrigger>)this._triggers).Count;
        public bool IsReadOnly => ((ICollection<MessageTrigger>)this._triggers).IsReadOnly;

        public void Add(MessageTrigger item)
        {
            ((ICollection<MessageTrigger>)this._triggers).Add(item);
        }

        public void Clear()
        {
            ((ICollection<MessageTrigger>)this._triggers).Clear();
        }

        public bool Contains(MessageTrigger item)
        {
            return ((ICollection<MessageTrigger>)this._triggers).Contains(item);
        }

        public void CopyTo(MessageTrigger[] array, int arrayIndex)
        {
            ((ICollection<MessageTrigger>)this._triggers).CopyTo(array, arrayIndex);
        }

        public bool Remove(MessageTrigger item)
        {
            return ((ICollection<MessageTrigger>)this._triggers).Remove(item);
        }

        public IEnumerator<MessageTrigger> GetEnumerator()
        {
            return ((IEnumerable<MessageTrigger>)this._triggers).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._triggers).GetEnumerator();
        }
        #endregion
    }
}
