using Infragistics.Windows.DockManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KB12060_WpfApp1;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // 「全てのペインをDocumentContentHostにタブでまとめる」ボタンのクリックイベントハンドラー
    private void putAllPanesInDocHostButton_Click(object sender, RoutedEventArgs e)
    {
        // DocumentContentHostを準備し、
        // DocumentContentHost、その最初の子のSplitPane、さらにその最初の子のTabGroupPaneを取得する。
        EnsureDocumentContentHost(xamDockManager1, true, true);
        var documentContentHost = (DocumentContentHost)xamDockManager1.Content;
        var rootSplitPane = (SplitPane)documentContentHost.Panes[0];
        var tabGroupPane = (TabGroupPane)rootSplitPane.Panes[0];

        // XamDockManagerが持っている全てのContentPaneを取り出し、List<T>オブジェクトとしてそのコピーを受け取る。
        // ※移動させながらforeachを回していくことになるので、移動処理の影響を受けないよう、コピーを使うようにします。
        var allContentPanes = xamDockManager1.GetPanes(PaneNavigationOrder.VisibleOrder).ToList();

        // XamDockManagerの各ContentPaneについて
        foreach (var contentPane in allContentPanes)
        {
            // XamDockManager
            // - DocumentContentHost
            // -- SplitPane
            // --- TabGroupPane
            // ---- ContentPane
            // ---- ContentPane
            // ...(以下、ContentPaneの繰り返し)...
            // という構造を作ると、DocumentContentHostにまとめてタブ表示にすることができます。
            // 
            // 以下のコードはこの構造を作成しているコードです。

            // DocumentContentHostのTabGroupPaneに含まれていないContentPaneの場合、
            if (!tabGroupPane.Items.Contains(contentPane))
            {
                // 現在の親Paneから削除し、...
                RemoveContentPane(contentPane);

                // TabGroupPaneに追加する。
                tabGroupPane.Items.Add(contentPane);
            }
        }

    }

    // XamDockManagerにDocumentContentHostを使用するために必要な要素があるかどうかチェックし、
    // なければ生成して追加し、DocumentContentHostが使えるようにするメソッドです。
    // 第1引数: 対象となるXamDockManager
    // 第2引数: DocumentContentHostの最初の子要素がSplitPaneかどうかについてもチェックと生成を行うかどうか。
    // 第3引数: DocumentContentHostの最初の子要素のSplitPaneのさらに最初の子要素がTabGroupPaneかどうかについてもチェックと生成を行うかどうか。第2引数がtrueの場合のみ有効な設定。
    private void EnsureDocumentContentHost(XamDockManager xamDockManager, bool populateRootSplitPane, bool populateRootTabGroupPane = false)
    {
        // DocumentContentHostがない場合は、生成してXamDockManagerに追加する。
        // ※注意
        // ※DocumentContentHost以外のContentがすでに存在しているかどうかのチェックはしていません。
        // ※要件によっては、DocumentContentHost以外のContentが存在しているかどうかチェックして、
        // ※すでにある場合はbool値などの「失敗」を示すコードを返す形に書き換えた方が良い場合もありますので、
        // ※そのあたりはご判断でお願いいたします。
        var documentContentHost = xamDockManager.Content as DocumentContentHost;
        if (documentContentHost == null)
        {
            documentContentHost = new DocumentContentHost();
            xamDockManager.Content = documentContentHost;
        }

        // DocumentContentHostの下にSplitPaneを用意する必要がない場合は、ここで処理終了。呼び出し元に戻る。
        if (!populateRootSplitPane) return;

        // DocumentContentHostにSplitPaneが少なくとも1個あるかチェックし、なければ生成してDocumentContentHostに追加する。
        // ※前提的知識として、DocumentContentHostのPanesにはSplitPaneしか置けません。
        // ※つまり、DocumentContentHostに1個でもPanesがあれば、それはSplitPaneです。
        if (documentContentHost.Panes.Count == 0)
        {
            documentContentHost.Panes.Add(new SplitPane());
        }

        // DocumentContentHostの子要素から最初のSplitPaneを取り出す。
        var rootSplitPane = (SplitPane)documentContentHost.Panes[0];

        // DocumentContentHostの最初の子要素のSplitPaneのそのさらに下の最初の子要素としてTabGroupPaneを用意する必要があるか？あれば用意する。
        if (populateRootTabGroupPane)
        {
            // 最初の子要素のSplitPaneの最初の子要素がTabGroupPaneでなければ、生成して最初の子要素として追加する。
            if (rootSplitPane.Panes[0].GetType() != typeof(TabGroupPane))
            {
                rootSplitPane.Panes.Insert(0, new TabGroupPane());
            }
        }
    }

    // ContentPaneを現在の親Paneから削除する処理をするメソッドです。
    // 第1引数: 対象となるContentPane
    private void RemoveContentPane(ContentPane contentPane)
    {
        // ContentPaneのもともとのCloseAction（ペインをクローズしたときにただクローズするか親Paneから削除もするかを指定するプロパティ）値を保存しておく。
        var originalCloseAction = contentPane.CloseAction;

        // 一時的にCloseActionをRemovePane（クローズと同時に親Paneからも削除する）値に変更する。
        contentPane.CloseAction = PaneCloseAction.RemovePane;

        // ContentPaneをクローズする（CloseActionの値に基づき、親Paneからも削除される）。
        contentPane.ExecuteCommand(ContentPaneCommands.Close);

        // もとのCloseActionに戻す。
        contentPane.CloseAction = originalCloseAction;
    }
}
