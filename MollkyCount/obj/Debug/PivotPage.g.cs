﻿

#pragma checksum "C:\Users\dverr.JM-BRUNEAU\documents\visual studio 2013\Projects\MollkyCount\MollkyCount\PivotPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "12E27EE61CF33D086105752D803C8BE4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MollkyCount
{
    partial class PivotPage : global::MollkyCount.Common.BasePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 52 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).ItemClick += this.ListView_ItemClick;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 57 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Holding += this.StackPanel_Holding;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

