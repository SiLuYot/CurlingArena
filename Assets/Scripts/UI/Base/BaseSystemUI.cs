using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public class BaseSystemUI : BaseUI
{
    public override void Close()
    {
        UIManager.Instance?.Close(index);
    }
}
