import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';

import '../../commons/dialogs.dart';

class WebViewPageController with ChangeNotifier {
  String requestUrl;
  bool isLoading = true;
  late final WebViewController controller;

  WebViewPageController(this.requestUrl);

  void initData() {
    controller = WebViewController()
      ..setNavigationDelegate(NavigationDelegate(
          onPageStarted: (url) {
            Dialogs.logInfo('onPageStarted: $url');
            isLoading = true;
            notifyListeners();
          },
          onProgress: (progress) {},
          onPageFinished: (url) {
            Dialogs.logInfo('onPageFinished: $url');
            isLoading = false;
            notifyListeners();
          },
          onWebResourceError: (error) {
            Dialogs.logInfo('onWebResourceError: ${error.description}');
          }))
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..addJavaScriptChannel('jsEcommerceChannel',
          onMessageReceived: (message) {})
      ..loadRequest(Uri.parse(requestUrl));
  }
}
