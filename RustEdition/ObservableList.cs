using System.Collections.Generic;
using System.ComponentModel;

internal class ObservableList<T> : List<T> where T : INotifyPropertyChanged
{
	public delegate void OnAdd(T item);

	public delegate void OnRemove(T item);

	public delegate void OnClear();

	public delegate void OnModify(T item, PropertyChangedEventArgs e);

	public event OnAdd onAdd;

	public event OnRemove onRemove;

	public event OnClear onClear;

	public event OnModify onModify;

	public new void Add(T item)
	{
		base.Add(item);
		onAdd?.Invoke(item);
		ref T reference = ref item;
		PropertyChangedEventHandler value = (object sender, PropertyChangedEventArgs e) =>
		{
			onModify?.Invoke(item, e);
		};
		reference.PropertyChanged += value;
	}

	public new void Remove(T item)
	{
		base.Remove(item);
		onRemove?.Invoke(item);
	}

	public new void Clear()
	{
		base.Clear();
		onClear?.Invoke();
	}
}
