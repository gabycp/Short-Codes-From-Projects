import { Injectable } from '@angular/core';
import { AlertController, LoadingController, ToastController } from '@ionic/angular';

@Injectable({
  providedIn: 'root'
})
export class UiServiceService {

  constructor(private alertController: AlertController,
              private loadingCtrl: LoadingController,
              private toastCtrl: ToastController ) { }

  async alertaInformativa(message: string) {
    const alert = await this.alertController.create({
      message,
      buttons: ['OK'],
    });

    await alert.present();
  }

  async loadingIU(message: string, show:boolean)
  {
    const loading = await this.loadingCtrl.create({
      message,
      
    });   

  }

  async presentToast(message) {
    const toast = await this.toastCtrl.create({
      message,
      duration: 2500,
      position: 'bottom',
    });

    await toast.present();
  }
}
