import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:webview_flutter/webview_flutter.dart';

import 'home-controller.dart';

class HomePage extends StatelessWidget {
  String requestUrl = 'https://flutter.dev';

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(create: (context) {
      var ctrl = HomeController(context, requestUrl);
      ctrl.initData();
      return ctrl;
    }, builder: (context, child) {
      return Scaffold(
        // appBar: AppBar(title: const Text('WebView Page')),
        body: ListenableBuilder(
            listenable: context.watch<HomeController>(),
            builder: (ctx, c) {
              return SafeArea(
                  child: ctx.watch<HomeController>().isLoading
                      ? const Center(
                          child: Text(
                            'Đang tải...',
                            style: TextStyle(color: Colors.red, fontSize: 18),
                          ),
                        )
                      : WebViewWidget(
                          controller: ctx.read<HomeController>().controller));
            }),
      );
    });
  }
}
