package chapter3.section2;

import pump.*;
import sodium.*;
import java.util.Optional;

public class LifeCyclePump implements Pump {
    public Outputs create(Inputs inputs) {
        LifeCycle lc = new LifeCycle(inputs.eNozzle1,
                                     inputs.eNozzle2,
                                     inputs.eNozzle3);
        return new Outputs()
            .setDelivery(lc.fillActive.map(
                of ->
                    of.equals(Optional.of(Fuel.ONE))   ? Delivery.FAST1 :
                    of.equals(Optional.of(Fuel.TWO))   ? Delivery.FAST2 :
                    of.equals(Optional.of(Fuel.THREE)) ? Delivery.FAST3 : 
                                                         Delivery.OFF))
            .setSaleQuantityLCD(lc.fillActive.map(
                of ->
                    of.equals(Optional.of(Fuel.ONE))   ? "1" :
                    of.equals(Optional.of(Fuel.TWO))   ? "2" :
                    of.equals(Optional.of(Fuel.THREE)) ? "3" : ""));
    }
}

