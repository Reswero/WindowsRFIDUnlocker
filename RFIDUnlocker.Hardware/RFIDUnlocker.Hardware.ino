#include <MFRC522.h>
#include <Keyboard.h>
#include <EEPROM.h>

#define DEBUG 1

#define READ_TIMEOUT_MS 5000

#if DEBUG == 1
  #define RESPONSE_TIMEOUT_MS 5000
#else
  #define RESPONSE_TIMEOUT_MS 200
#endif

#define ACTION_ENTERED 0
#define ACTION_NOT_ENTERED 1

#define RFID_SS_PIN 10
#define RFID_RST_PIN 2

#define RED_LED_PIN 9
#define GREEN_LED_PIN 6
#define BLUE_LED_PIN 5

#define MAX_LED_BRIGHTNESS 30

#define BUZZER_PIN 3
#define MAX_BUZZER_FREQUENCY 100

struct {
  char password[128];
} securitySettings;

MFRC522 RFID(RFID_SS_PIN, RFID_RST_PIN);

long lastTime = 0;

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(200);

  SPI.begin();
  RFID.PCD_Init();
}

void loop() {
  if (Serial.available() > 0) {
    String message = Serial.readString();

    if (message.startsWith("<?")) {
      handleRequest(message);
    }

    delay(2000);
  }

  if (isCardDetected()) {
    String uid = "";
    
    for (int i = 0; i < RFID.uid.size; i++) {
      uid += String(RFID.uid.uidByte[i], HEX);
    }

    uid.toUpperCase();

#if DEBUG == 1
    Serial.println(uid);
#endif

    if (checkCardAccess(uid)) {
      grantAccess();
      logAction(uid, ACTION_ENTERED);
    }
    else {
      denyAccess();
      logAction(uid, ACTION_NOT_ENTERED);
    }

    delay(2000);
  }
}

void handleRequest(String request) {
  String command = request.substring(2, 5);
  
  if (command == "add") {
    addCard();
  }
  else if (command = "psw") {
    int startIndex = 19;
    int endIndex = request.lastIndexOf("\"");

    String password = request.substring(startIndex, endIndex);

    setPassword(password);
  }
}

void addCard() {
  analogWrite(RED_LED_PIN, MAX_LED_BRIGHTNESS * 0.8);
  analogWrite(GREEN_LED_PIN, MAX_LED_BRIGHTNESS * 0.6);

  String response;
  long startTime = millis();

  while (!isCardDetected() && (millis() - startTime <= READ_TIMEOUT_MS)) {}

  if (millis() - startTime < READ_TIMEOUT_MS) {
    String uid = "";
    for (int i = 0; i < RFID.uid.size; i++) {
      uid += String(RFID.uid.uidByte[i], HEX);
    }
    uid.toUpperCase();

    response = "<!add10|{\"uid\":\"" + uid + "\"}>";
  }
  else {
    response = "<!add11>";
  }
  
  Serial.println(response);
  resetAllLeds();
}

bool checkCardAccess(String uid) {
  String request = "<?acs|{\"uid\":\"" + uid + "\"}>";
  Serial.println(request);

  long startTime = millis();

  while (Serial.available() == 0 && (millis() - startTime <= RESPONSE_TIMEOUT_MS)) {}

  if (Serial.available() > 0) {
    String response = Serial.readString();

    if (response.startsWith("<!acs10>")) {
      return true;
    }
  }
  
  return false;
}

void logAction(String uid, int action) {
  String request = "<?log|{\"uid\":\"" + uid + "\", \"action\":\"" + action + "\"}>";
  Serial.println(request);
}

void setPassword(String password) {
  password.toCharArray(securitySettings.password, 128);
  EEPROM.put(0, securitySettings);

  Serial.write("<!psw10>");

#if DEBUG == 1
  Serial.println(securitySettings.password);
  EEPROM.get(0, securitySettings);
  Serial.println(securitySettings.password);
  String pass = String(securitySettings.password);
  Serial.println(pass.length());
#endif
}

void getPassword() {
  EEPROM.get(0, securitySettings);
}

bool isCardDetected() {
  if (!RFID.PICC_IsNewCardPresent()) {
    return false;
  }

  if (!RFID.PICC_ReadCardSerial()) {
    return false;
  }
  
  return true;
}

void grantAccess() {
  blinkLed(GREEN_LED_PIN);
  playAccessSound();
  enterPassword();
}

void denyAccess() {
  blinkLed(RED_LED_PIN);
  playDenySound();
}

void blinkLed(int ledPin) {
  resetAllLeds();

  for (int i = 1; i < 3; i++) {
    analogWrite(ledPin, MAX_LED_BRIGHTNESS);
    delay(100);
    analogWrite(ledPin, 0);
    delay(100);
  }
}

void resetAllLeds() {
  analogWrite(RED_LED_PIN, 0);
  analogWrite(GREEN_LED_PIN, 0);
  analogWrite(BLUE_LED_PIN, 0);
}

void enterPassword() {
  getPassword();
  
  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
  delay(2000);

  Keyboard.print(securitySettings.password);
  delay(100);

  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
}

void playAccessSound() {
  for (int i = 0; i < 3; i++) {
    delay(100);
    tone(BUZZER_PIN, MAX_BUZZER_FREQUENCY);
    delay(100);
    noTone(BUZZER_PIN);
  }
}

void playDenySound() {
  for (int i = 0; i < 2; i++) {
    delay(100);
    tone(BUZZER_PIN, MAX_BUZZER_FREQUENCY);
    delay(200);
    noTone(BUZZER_PIN);
  }
}
