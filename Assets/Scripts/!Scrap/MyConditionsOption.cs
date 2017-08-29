using UnityEngine;

namespace MyCustomDrawers {

#if UNITY_EDITOR
	
	using UnityEditor;
	public class Enumeration{
		
		public static readonly string[] items;
		
	}
	
	//Example 1
	public class ConditionTypes : Enumeration{
		
		public static readonly new string[] items = new string[]{ "default", "options", "completequest","first","noquest","unfinishedquest","hascompletedquest","handedinquest" };
		
	}

	public class Enum : PropertyAttribute{
		
		public readonly string[] items;
		public int selected = 0;
		
		public Enum(System.Type type){
			
			if (type.IsSubclassOf(typeof(Enumeration)))
			{
				System.Reflection.FieldInfo fieldInfo = type.GetField("items");
				this.items = (string[])fieldInfo.GetValue (null);
				
			} else {
				
				this.items = new string[]{"Assign Enumeration Type"};
				
			}  
			
		}
		
		public Enum(params string[] enumerations){ this.items = enumerations; }
		
	}
	[CustomPropertyDrawer (typeof (Enum))]
	public class EnumDrawer : PropertyDrawer {

		
		Enum enumeration{ get{ return (Enum)attribute; } }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) {
			
			bool Start = true;
			if(Start)
			{
				
				Start = false;
				for(int i=0;i<enumeration.items.Length;i++){
					
					if(enumeration.items[i].Equals(prop.stringValue)){
						
						enumeration.selected = i;
						break;
						
					}
					
				}
				
			}
			
			enumeration.selected = EditorGUI.Popup(EditorGUI.PrefixLabel(position, label),enumeration.selected,enumeration.items);
			prop.stringValue = enumeration.items[enumeration.selected];
			
		}
	}
	#endif
	/*public class ConditionsOption : PropertyAttribute
	{
	}
	[CustomPropertyDrawer(typeof(ConditionsOption))]
	public class MyConditionsOptionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			//string valueStr;
			int selected = 0;
			string[] options = new string[]
			{
				"Option1", "Option2", "Option3", 
			};
			selected = EditorGUILayout.Popup("Label", selected, options); 
			prop.stringValue = options [selected];
			
			EditorGUI.LabelField(position,label.text, prop.stringValue);
		}
	}*/
}    