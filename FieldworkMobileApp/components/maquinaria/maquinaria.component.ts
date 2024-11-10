import { Component, Input, OnInit } from '@angular/core';
import { ViewWillEnter } from '@ionic/angular';
import { DatabaseService } from 'src/app/services/database.service';
import { EjecucionProyecto } from 'src/interfaces/interfaces';

@Component({
  selector: 'app-maquinaria',
  templateUrl: './maquinaria.component.html',
  styleUrls: ['./maquinaria.component.scss'],
})
export class MaquinariaComponent  implements OnInit, ViewWillEnter {

  @Input() maquinaria1: EjecucionProyecto = {};
  
  horasPendiente: number = 0;
  constructor(private dbService: DatabaseService) { }

  ngOnInit(){
    this.ReadHorasPendiente(this.maquinaria1.IdEjecucionProyecto.toString());
  }

  ionViewWillEnter()
  {
    
  }

  async ReadHorasPendiente(IdEjecucionProyecto)
  {
    try
    {     
      const horasP = await this.dbService.getHorasPendiente(IdEjecucionProyecto);
      this.horasPendiente = horasP;
    }
    catch (err) {
      console.error(err);
      console.error("Error al leer horas");
    }
    
    
  }

}
