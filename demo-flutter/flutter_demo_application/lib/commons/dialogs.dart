import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import 'toast-type.dart';

class Dialogs {
  static void logInfo(Object? object) {
    if (kDebugMode) {
      print(object);
    }
  }

  // toast
  static Future toast(BuildContext ctx, String msg,
      {String? title,
      int? milliseconds,
      int? seconds,
      ToastType? toastType}) async {
    Color bgColor = Colors.green;
    toastType = toastType ?? ToastType.success;
    switch (toastType) {
      case ToastType.success:
        bgColor = Colors.green;
        break;
      case ToastType.error:
        bgColor = Colors.red;
        break;
    }
    await Future.sync(() {
      final snackbar = SnackBar(
        content: Text(msg),
        backgroundColor: bgColor,
      );
      ScaffoldMessenger.of(ctx).showSnackBar(snackbar);
    });
  }

  // alert
  static Future alert(BuildContext ctx, String msg,
      {String? title, String? okText}) async {
    await Future.sync(() {
      showDialog(
          context: ctx,
          builder: (context) {
            return AlertDialog(
              title: title != null ? Text(title) : null,
              content: Text(msg),
              actions: <Widget>[
                TextButton(
                  child: Text(okText ?? 'OK'),
                  onPressed: () {
                    Navigator.of(context).pop();
                  },
                ),
              ],
            );
          });
    });
  }

  // confirm
  static Future<bool?> confirm(BuildContext ctx, String msg,
      {String? title, String? okText, String? cancelText}) async {
    return await showDialog<bool>(
        context: ctx,
        builder: (context) {
          return AlertDialog(
            title: title != null ? Text(title) : null,
            content: Text(msg),
            actions: <Widget>[
              TextButton(
                child: Text(cancelText ?? 'Cancel'),
                onPressed: () {
                  Navigator.of(context).pop(false);
                },
              ),
              TextButton(
                child: Text(okText ?? 'OK'),
                onPressed: () {
                  Navigator.of(context).pop(true);
                },
              ),
            ],
          );
        });
  }
}
