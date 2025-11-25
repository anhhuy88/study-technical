import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

class AppStorage {
  static Future<void> setData(String key, Object value) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();
    String jsonString = json.encode(value);
    await prefs.setString(key, jsonString);
  }

  static Future<dynamic> getData(String key) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();
    String? jsonString = prefs.getString(key);
    if (jsonString != null && jsonString.isNotEmpty) {
      return json.decode(jsonString);
    }
    return null;
  }

  static Future<void> removeData(String key) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.remove(key);
  }
}
