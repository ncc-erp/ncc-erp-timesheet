import { Injectable } from "@angular/core";
import { from, Observable } from "rxjs"; // 1. Import `from` và `Observable`
import { mmnClient } from "@shared/mmn-clients";

@Injectable({ providedIn: "root" })
export class MmnTestService {
  private mmn = mmnClient;
  transfer(amountToTransfer: number): Observable<any> {
    return from(this.executeTransfer(amountToTransfer));
  }
  async executeTransfer(amountToTransfer: number) {
    const senderUserId = localStorage.getItem("mezonUserId");
    const senderAddress = this.mmn.getAddressFromUserId(senderUserId);
    const recipientAddress = "HsvGsttQ8swfZehVDMDYqRWEMA8CW6AfyLjk3jBwQagY";
    const keyPairRaw = localStorage.getItem('keyPair');
    const keyPair = keyPairRaw ? JSON.parse(keyPairRaw) : undefined;
    const zkProofRaw = localStorage.getItem('zkProof');
    const zkProof = zkProofRaw ? JSON.parse(zkProofRaw) : undefined;
    try {
      const nonceRes = await this.mmn.getCurrentNonce(senderUserId);
      const nonce = nonceRes.nonce + 1;
      const amount = this.mmn.scaleAmountToDecimals(amountToTransfer);
      const res = await this.mmn.sendTransactionByAddress({
        sender: senderAddress,
        recipient: recipientAddress,
        amount,
        nonce,
        privateKey: keyPair.privateKey,
        publicKey: keyPair.publicKey,
        zkProof: zkProof.proof,
        zkPub: zkProof.public_input,
        textData: `Tranfer ${amountToTransfer} from timesheet`,
        extraInfo: { type: "transfer_token" },
      });

      if (res.ok) {
        const result= { amount: amountToTransfer, txhash: res.tx_hash }
        return result;
      }
      console.log("Transfer result:", res);
    } catch (e) {
      console.error("Transfer failed:", e);
    }
  }
}
