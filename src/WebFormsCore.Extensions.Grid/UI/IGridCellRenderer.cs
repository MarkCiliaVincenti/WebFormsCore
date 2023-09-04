﻿using System.Reflection;

namespace WebFormsCore.UI;

public interface IGridCellRenderer
{
    bool SupportsType(PropertyInfo property);

    ValueTask CellCreated(PropertyInfo property, TableCell cell, GridItem item);

    ValueTask CellDataBinding(PropertyInfo property, TableCell cell, GridItem item, object? value);
}
