using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace react_snippet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<string> ActionTypesFormated { get; set; }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            var folder = FolderTextbox.Text;
            var actionTypes = ActionTypesTextbox.Text.ToUpper().Split(';').ToList();
            GenerateSnippets(folder, actionTypes);
        }

        private void GenerateSnippets(string folder, List<string> actionTypes)
        {
            var actionTypesFile = CreateActionTypes(folder, actionTypes);
            // Create the file.
            using (var fs = File.Create($"{folder}ActionTypes.js"))
            {
                var info = new UTF8Encoding(true).GetBytes(actionTypesFile);
                fs.Write(info, 0, info.Length);
            }

            var reducer = CreateReducer(folder, actionTypes);
            // Create the file.
            using (var fs = File.Create($"{folder}Reducer.js"))
            {
                var info = new UTF8Encoding(true).GetBytes(reducer);
                fs.Write(info, 0, info.Length);
            }

            var initialState = CreateInitialState();
            using (var fs = File.Create($"{folder}InitialState.js"))
            {
                var info = new UTF8Encoding(true).GetBytes(initialState);
                fs.Write(info, 0, info.Length);
            }

            var action = CreateActions(folder, actionTypes);
            using (var fs = File.Create($"{folder}Action.js"))
            {
                var info = new UTF8Encoding(true).GetBytes(action);
                fs.Write(info, 0, info.Length);
            }

            MessageBox.Show("Check local folder", "Done", MessageBoxButtons.OK);
        }

        private string CreateActions(string folderCamel, List<string> actionTypes)
        {
            var sb = new StringBuilder();
            var folderLower = char.ToLower(folderCamel[0]) + folderCamel.Substring(1);
            var import1 = $@"import * as Types from './{folderCamel}ActionTypes';";
            sb.AppendLine(import1).AppendLine();
            int i = 0;
            foreach (var item in actionTypes)
            {
                var exportFunction = $@"export function {folderLower}{ToCamelCase(item)}() {{";
                var returnType = $@"    return {{type: Types.{ActionTypesFormated[i]}}};";
                sb.AppendLine(exportFunction);
                sb.AppendLine(returnType).AppendLine("}").AppendLine();
                i++;
            }

            return sb.ToString();
        }

        private string ToCamelCase(string text)
        {
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
        private string CreateInitialState()
        {
            var exportDefault = @"export default {";
            var isBusy = @"    isBusy: false";
            var sb = new StringBuilder();
            sb.AppendLine(exportDefault);
            sb.AppendLine(isBusy).AppendLine("}");
            return sb.ToString();
        }

        private string CreateReducer(string folderCamel, List<string> actionTypes)
        {
            var folderLower = char.ToLower(folderCamel[0]) + folderCamel.Substring(1);
            var import1 = $@"import * as Types from './{folderCamel}ActionTypes';";
            var import2 = $@"import initialState from './{folderCamel}InitialState';";

            var exportDefault = $@"export default function {folderLower}Reducer(state = initialState, action) {{";
            var initSwitch = $@"    switch (action.type) {{";
            var returnItem = $@"            return {{ ...state, isBusy: }};";

            var sb = new StringBuilder();
            sb.AppendLine(import1);
            sb.AppendLine(import2).AppendLine();
            sb.AppendLine(exportDefault);
            sb.AppendLine(initSwitch);
            foreach (var item in ActionTypesFormated)
            {
                var caseItem = $@"      case Types.{item}:";
                sb.AppendLine(caseItem);
                sb.AppendLine(returnItem);
            }

            var defaultItem = $@"       default:";
            var defaultReturn = $@"     return state;";
            sb.AppendLine(defaultItem).AppendLine(defaultReturn).AppendLine(@"  }").AppendLine(@"}");
            return sb.ToString();
        }

        private string CreateActionTypes(string folderCamel, List<string> actionTypes)
        {
            ActionTypesFormated = new List<string>();
            var folder_snake = string.Concat(folderCamel.Select((x, i) => i > 0
                                                                          && char.IsUpper(x)
                ? "_" + x.ToString()
                : x.ToString())).ToUpper();
            var sb = new StringBuilder();
            foreach (var item in actionTypes)
            {
                var placeHolder = $@"export const {folder_snake}_{item} = '{folderCamel}/{item}';";
                ActionTypesFormated.Add($@"{folder_snake}_{item}");
                sb.AppendLine(placeHolder);
            }

            return sb.ToString();
        }
    }
}