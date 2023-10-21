using SystemSimulation;

SystemSimulationTests.Tests.test_all();
Console.WriteLine("Tests finished");


// example for the schematics below
// [sensor1]---[valve1]---|
//                        | (1oo2) ---|
// [sensor2]--------------|           | (2oo2) ----> 
// [sensor3]--------------------------|

SILSystem system = new SILSystem();

// create element types and specify failure rates
var sensor = system.add_element_type(new ElementType(
    // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
    0.001, 0.0003, 0.8, 0.02, 0.01, 8, 8
));

var valve = system.add_element_type(new ElementType(
    // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
    0.001, 0.001, 0.6, 0.02, 0.01, 8, 8
));

// create individual elements
var sensor1 = system.add_element(sensor);
var sensor2 = system.add_element(sensor);
var sensor3 = system.add_element(sensor);
var valve1 = system.add_element(valve);

// connect all elements using votings

// connecting elements in series is same as voting without redundancy
var connection1 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor1, valve1}).ToList(), (new Voting[] {}).ToList(), 0
);

var voting1oo2 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor2}).ToList(), (new Voting[] {connection1}).ToList(), 1
);

var voting2oo2 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor3}).ToList(), (new Voting[] {voting1oo2}).ToList(), 1
);

// specify which voting produces the final output
system.set_final_voting(voting2oo2);

// compile the system
system.compile();

// now we can get failure probabilities at certain times
Console.WriteLine(
    "Failure probability after 1000 hrs (200 hrs after last proof test): {0}",
    system.failure_probability(1000, 200)
);

// and it is also possible to integrate and average the probability
Console.WriteLine(
    "Average failure probability after 1000 hrs (proof test every 400 hrs): {0}",
    system.average_failure_probability(1000, 400)
);





