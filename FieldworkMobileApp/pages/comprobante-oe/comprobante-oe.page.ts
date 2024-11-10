import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavController } from '@ionic/angular';
import { DatabaseService } from 'src/app/services/database.service';
import { UiServiceService } from 'src/app/services/ui-service.service';
import { Bancos, OrdenesTemporales } from 'src/interfaces/interfaces';

@Component({
  selector: 'app-comprobante-oe',
  templateUrl: './comprobante-oe.page.html',
  styleUrls: ['./comprobante-oe.page.scss'],
})
export class ComprobanteOEPage implements OnInit {

  ordenEspecial: OrdenesTemporales;
  //listBancos:Bancos[] = [ { "IdBanco": 1, "DescripcionBanco": "Banco Atlatida" }, { "IdBanco": 2, "DescripcionBanco": "Banco Ficohsa" }, { "IdBanco": 3, "DescripcionBanco": "Bac Credomatic" }, { "IdBanco": 4, "DescripcionBanco": "Banco Occidente" } ];
  listBancos:Bancos[] = [];
  usuario: string = '';
  fotografiaNueva: string ='';
  maxDate: string = '';
 
  constructor(private route:ActivatedRoute,
              private navCtrl: NavController,
              private dbService: DatabaseService,
              private uiService: UiServiceService) 
  {

    this.route.queryParams.subscribe(params => {
      //console.log(params);
      //console.log(JSON.stringify(params));
      this.ordenEspecial = JSON.parse(params["ordenespecial"]);
      if(!this.ordenEspecial)
      {
        this.navCtrl.back( {animated:true} );
      } 
      
      this.dbService.GetFotografiaOrdenEspecial(this.ordenEspecial.CodigoOrdenTemporal)
            .then((changes) =>
            {
              console.log("Esta es la fotografia desde comprobante" + JSON.stringify(changes));
              
              this.fotografiaNueva = changes;
            })
            .catch(err => Promise.reject(err));

  });
  
  
  }

  async ngOnInit() {
    const today = new Date();
    this.maxDate = today.toISOString().split('T')[0];

    this.usuario = await this.dbService.getCurrentUsername();
    this.initList();

  }

  initList()
  {
    this.dbService.getBancos().then((bancos: Bancos[]) =>
    {
      this.listBancos.push(...bancos);
    })
    .catch( err => Promise.reject(err));

  }

  handleChangeBanco(e)
  {
    console.log("Este es el valor banco: " + this.ordenEspecial.IdBanco);
  }

  handlePhotoTaken(imageData: string) {
    // Hacer algo con el imageData capturado desde el PhotoComponent
    console.log('ImageData obtenido:', imageData);
    this.fotografiaNueva = imageData;
  }

  async guardarOrdenEspecial()
  {
    const fechaHoy: Date = new Date();
    const fechaFormateada: string = fechaHoy.toISOString().split('T')[0];
    console.log("Esta es la orden:");
    this.ordenEspecial.CreadoPor = this.usuario;
    this.ordenEspecial.FechaCreacion = fechaFormateada;
    if(this.ordenEspecial.Fecha)
    {
      const fechaConvertida: Date = new Date(this.ordenEspecial.Fecha);
      this.ordenEspecial.Fecha = fechaConvertida.toISOString().split('T')[0];
    }

    if(await this.validarForm())
    {   
      console.log(JSON.stringify(this.ordenEspecial));
      this.dbService.InsertOrdenEspecial(this.ordenEspecial)
      .then( (changes) =>{
        console.log(JSON.stringify(changes));
        console.log("Creado orden especial por movil");
        
        if(this.fotografiaNueva != '')
        {
          this.dbService.InsertFotograficaOrdenEspecial(this.ordenEspecial.CodigoOrdenTemporal, this.fotografiaNueva)
          .then((changes) => 
          {
            console.log(JSON.stringify(changes));
            this.uiService.presentToast('Orden especial creada/actualizada!');
            this.navCtrl.navigateBack('/main/tabs/tab2', {animated:true});  
          })
          .catch(err => {
            console.error(err);
            console.error("Error al crear fotografia orden especial");
          });
        }else
        { 
          this.uiService.presentToast('Orden especial creada/actualizada!');
          this.navCtrl.navigateBack('/main/tabs/tab2', {animated:true});  
        }
            
      })
      .catch(err => {
        console.error(err);
        console.error("Error al crear orden especial");
      });

    }
  }

  async validarForm(): Promise<boolean> 
  {
    if (this.ordenEspecial.NumeroDeposito == null || this.ordenEspecial.NumeroDeposito == '') {
      this.uiService.presentToast('Requiere el Número de Deposito');
      return false;
    }
    else if (this.ordenEspecial.Fecha == null ) {
      this.uiService.presentToast('Requiere de una fecha');
      return false;
    }
    else if (this.ordenEspecial.Monto == null || this.ordenEspecial.Monto < 0) {
      this.uiService.presentToast('Requiere de un monto');
      return false;
    }
    else if (this.ordenEspecial.IdBanco == null) {
      this.uiService.presentToast('Debe seleccionar un banco');
      return false;
    }
    else if (this.fotografiaNueva == null || this.fotografiaNueva == '') {
      this.uiService.presentToast('Requiere la fotografia del comprobante');
      return false;
    }
    else
    {
      return true;
    }
  }


  back()
  {
    this.navCtrl.navigateBack(`/form-orden-t/${this.ordenEspecial.CodigoOrdenTemporal}`,{ animated: true })
  }

}
