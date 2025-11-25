import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:webview_flutter/webview_flutter.dart';

import 'webview-page-controller.dart';

class WebViewPage extends StatelessWidget {
  String requestUrl;
  WebViewPage(this.requestUrl, {super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (context) {
        var ctrl = WebViewPageController(requestUrl);
        ctrl.initData();
        return ctrl;
      },
      builder: (context, c) {
        return Scaffold(
          appBar: AppBar(title: const Text('WebView Page')),
          body: ListenableBuilder(
              listenable: context.watch<WebViewPageController>(),
              builder: (ctx, c) {
                return SafeArea(
                    child: ctx.watch<WebViewPageController>().isLoading
                        ? const Center(
                            child: Text(
                              'Đang tải...',
                              style: TextStyle(color: Colors.red, fontSize: 18),
                            ),
                          )
                        : WebViewWidget(
                            controller:
                                ctx.read<WebViewPageController>().controller));
              }),
        );
      },
    );
  }
}
