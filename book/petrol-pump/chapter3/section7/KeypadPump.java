package chapter3.section7;

import pump.*;
import sodium.*;
import java.util.Optional;

public class KeypadPump implements Pump
{
    public Outputs create(Inputs inputs) {
        Keypad ke = new Keypad(inputs.eKeypad, new Event<Unit>());
        return new Outputs()
            .setPresetLCD(ke.value.map(v ->
                Formatters.formatSaleCost((double)v)))
            .setBeep(ke.eBeep);
    }
}

