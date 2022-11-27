#include <MFRC522.h>

#define RFID_SS_Pin 10
#define RFID_RST_Pin 2

MFRC522 RFID(RFID_SS_Pin, RFID_RST_Pin);

void setup() {
  Serial.begin(9600);

  SPI.begin();
  RFID.PCD_Init();
}

void loop() {
  if (IsCardDetected()) {
    for (int i = 0; i < RFID.uid.size; i++) {
      Serial.print(RFID.uid.uidByte[i], HEX);
      Serial.print(" ");
    }
    Serial.println();
  }
}

bool IsCardDetected() {
  if (!RFID.PICC_IsNewCardPresent()) {
    return false;
  }

  if (!RFID.PICC_ReadCardSerial()) {
    return false;
  }
  
  return true;
}
