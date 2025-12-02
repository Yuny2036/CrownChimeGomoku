using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;



public class Queue
{
	struct Info
	{
		public int offset;
		public int size;
	};

	private MemoryStream buffer;
	private List<Info> list;
	private int offset = 0;

	private Object lockObj = new Object();

	public Queue()
	{
		buffer = new MemoryStream();
		list = new List<Info>();
	}

	public int Add(byte[] data, int size)
	{
		Info info = new Info();

		info.offset = offset;
		info.size = size;

		lock (lockObj)
		{
			list.Add(info);

			buffer.Position = offset;
			buffer.Write(data, 0, size);
			buffer.Flush();
			offset += size;
		}

		return size;
	}

	public int Pop(ref byte[] data, int size)
	{
		if (list.Count <= 0)
		{
			return -1;
		}

		int iSize = 0;
		lock (lockObj)
		{
			Info info = list[0];

			int dataSize = Math.Min(size, info.size);
			buffer.Position = info.offset;
			iSize = buffer.Read(data, 0, dataSize);

			if (iSize > 0)
			{
				list.RemoveAt(0);
			}

			if (list.Count == 0)
			{
				Clear();
				offset = 0;
			}
		}

		return iSize;
	}

	public void Clear()
	{
		byte[] data = buffer.GetBuffer();
		Array.Clear(data, 0, data.Length);

		buffer.Position = 0;
		buffer.SetLength(0);
	}

	    public void PutInInventory(ItemInstance itemInstance)
    {
        switch (itemInstance)
        {
            case EquipmentItemInstance eii:
                Debug.LogWarning($"2 {eii.ItemName}, {eii.UniqueID}");
                if (InventoryList.Contains(eii)) throw new ArgumentException("You're trying to put the exact same entity in. How did you do..?");
                PutItemInNewSlot(eii);

                break;

            case PropsItemInstance pii:
                Debug.LogWarning("2 Props");
                var existingItem = InventoryList
                .OfType<PropsItemInstance>()
                .Where(p => p.baseData.baseID == pii.baseData.baseID)
                .ToList();
                // .FirstOrDefault(item => item.baseData.baseID == pii.baseData.baseID);

                if (existingItem.Count != 0)
                {
                    int spaceLeft;
                    foreach (var prop in existingItem)
                    {
                        spaceLeft = prop.baseData.MaximumStacks - prop.CurrentStacks;

                        if (spaceLeft <= 0) continue;

                        if (pii.CurrentStacks <= spaceLeft)
                        {
                            prop.CurrentStacks += pii.CurrentStacks;
                        }
                        else
                        {
                            prop.CurrentStacks += spaceLeft;

                            var remainingItem = new PropsItemInstance(pii.baseData, pii.CurrentStacks - spaceLeft);

                            PutItemInNewSlot(remainingItem);
                        }
                    }
                }
                else
                {
                    PutItemInNewSlot(pii);
                }
                break;
        }

        ShowItemInInventory();
    }
}