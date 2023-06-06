using System;

namespace Runtime.SaveLoad {
    [Serializable]
    public sealed class SaveProfile<T> where T : SaveData {
        public string profileName;
        public T saveData;

        private SaveProfile() { }

        public SaveProfile(string profileName, T saveData) {
            this.profileName = profileName;
            this.saveData = saveData;
        }
    }

    public abstract record SaveData;
}