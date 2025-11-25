import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';

import '../../commons/dialogs.dart';
import '../../commons/variables.dart';
import '../webview-page/webview-page.dart';

class HomeController with ChangeNotifier {
  bool isLoading = true;
  String requestUrl;
  late final WebViewController controller;

  final BuildContext _context;

  HomeController(this._context, this.requestUrl);

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
      ..addJavaScriptChannel('jsOmsChannel', onMessageReceived: (dataMsg) {
        Dialogs.logInfo('jsOmsChannel:onMessageReceived: $dataMsg');
        if (dataMsg.message.isNotEmpty) {
          var data = json.decode(dataMsg.message) as Map;
          switch (data[Variables.ACTION_FIELD]) {
            case Variables.NAVIGATOR_MSG_TYPE:
              goToWebViewPage(data[Variables.URL_FIELD]);
              break;
            default:
              break;
          }
        }
      })
      ..loadRequest(Uri.parse(requestUrl));
  }

  void goToWebViewPage(String url) {
    Navigator.push(
        _context, MaterialPageRoute(builder: (context) => WebViewPage(url)));
  }
}
