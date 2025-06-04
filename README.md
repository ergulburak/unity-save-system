
# 📦 Unity Save System (UPM Package)

## Overview

This package provides a modular, slot-based save system for Unity projects. It uses asynchronous operations, memory caching, and a helper interface for clean and reusable save/load workflows.

---

## 📥 Installation

To install via **Unity Package Manager**, add the following line to your `Packages/manifest.json`:

```json
"com.ergulburak.savesystem": "https://github.com/ergulburak/unity-save-system.git?path=/SaveSystem"
```

> ⚠️ Make sure the package repository contains a valid `package.json` in the `SaveSystem/` folder.

---

## 🚀 Usage Guide

### 1. Initialization

Call once during your game's boot sequence to load current save slot and data.

```csharp
SaveHelper.Initialize();
```

You can listen for completion:

```csharp
SaveHelper.OnInitializeComplete += slotId => {
    Debug.Log($"Save system initialized for slot: {slotId}");
};
```

---

### 2. Getting Save Data

Use `GetData<T>()` to access cached save data for your custom class that implements `ISaveable`:

```csharp
var myData = SaveHelper.GetData<MySaveData>();
```

---

### 3. Saving Specific Data

Each class implementing `ISaveable` can call extension method:

```csharp
myData.SaveData(() => {
    Debug.Log("MyData saved!");
});
```

---

### 4. Saving All Data

To save all registered saveable objects:

```csharp
SaveHelper.SaveGame(() => {
    Debug.Log("All data saved.");
});
```

---

### 5. Switching Save Slots

You can dynamically change save slots like this:

```csharp
SaveHelper.ChangeSaveSlot(1, () => {
    Debug.Log("Switched to slot 1");
});
```

---

## 🧱 Interfaces

### `ISaveable`

Implement this interface in any data class you wish to save. Must be serializable and unique.

---

## 📂 Project Structure

```
SaveSystem/
├── Runtime/
│   └── SaveHelper.cs
│   └── SaveManager.cs
│   └── SaveSystem.cs
│   └── ISaveable.cs
├── Editor/
├── package.json
```

---

## 📄 License & Credits

Created by **Burak Ergül**  
GitHub: [github.com/ergulburak](https://github.com/ergulburak)
