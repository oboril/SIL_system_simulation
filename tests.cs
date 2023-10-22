namespace SystemSimulationTests
{
    using System.Reflection.Emit;
    using SystemSimulation;

    static class Tests
    {
        private static void assert_close(double val1, double val2, double rtol, string message = "")
        {
            double tol = (Math.Abs(val1) + Math.Abs(val2)) / 2 * rtol;
            if (Math.Abs(val1 - val2) > tol)
            {
                Console.WriteLine("val1: {0}, val2: {1}, tol: {2}", val1, val2, tol);
                throw new Exception("Assertion failed: " + message);
            }
        }

        private static double exponential_dist(double x)
        {
            // Using Taylor expansion of 1 - exp(-rt) is most numerically stable
            if (x < 1e-5)
                // Using Taylor expansion of 1 - exp(-x) is most numerically stable
                return x - x * x / 2 + x * x * x / 6 - x * x * x * x / 24;
            else
                // for larger x the exact formula is better
                return 1 - Math.Exp(-x);
        }

        public static void test_all()
        {
            test1();
            test2();
            test3();
            test4();
            test5();
            test6();
            test7();
            test8();
            test9();
            test10();
            test11();
            test12();
        }

        static void test1()
        {
            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.0, 0.01, 0, 0, 0, 0, 0, 0.5
            ));

            var e1 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1 }).ToList(), (new Voting[] { }).ToList(), 0);

            system.set_final_voting(v1);

            system.compile();

            assert_close(system.failure_probability(1), exponential_dist(0.01), 1e-5);
            assert_close(system.failure_probability(2), exponential_dist(0.02), 1e-5);
        }

        static void test2()
        {
            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.0, 0.01, 1, 0, 0, 0, 0, 2
            ));

            var e1 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1 }).ToList(), (new Voting[] { }).ToList(), 0);

            system.set_final_voting(v1);

            system.compile();

            assert_close(system.failure_probability(0), exponential_dist(0.0), 1e-8);
            assert_close(system.failure_probability(1), exponential_dist(0.01), 1e-8);
        }

        static void test3()
        {
            // 2oo2

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.0, 0.01, 0, 0, 0, 0, 0, 1
            ));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1, e2 }).ToList(), (new Voting[] { }).ToList(), 0);

            system.set_final_voting(v1);

            system.compile();

            double p = exponential_dist(0.01);

            assert_close(system.failure_probability(1), 2 * p - p * p, 1e-8);
        }

        static void test4()
        {
            // 1oo2

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.0, 0.01, 0, 0, 0, 0, 0, 1
            ));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1, e2 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v1);

            system.compile();

            double p = exponential_dist(0.01);

            assert_close(system.failure_probability(1), p * p, 1e-8);
        }

        static void test5()
        {
            // common failure

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.0, 0.01, 0, 0, 0.3, 0, 0, 1
            ));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1, e2 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v1);

            system.compile();

            double pc = exponential_dist(0.003);
            double pi = exponential_dist(0.007);

            assert_close(system.failure_probability(1), pc + pi * pi - pc * pi * pi, 1e-8);
        }

        static void test6()
        {
            // mrt, mttr

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0.03, 0.01, 0, 0, 0, 9, 8, 1
            ));

            var e1 = system.add_element(et1);

            var v1 = new Voting((new int[] { e1 }).ToList(), (new Voting[] { }).ToList(), 0);

            system.set_final_voting(v1);

            system.compile();

            double p = exponential_dist(0.01);

            assert_close(system.failure_probability(1), 0.03 * 9 + 0.01 * 8 + p, 1e-8);
        }

        static void test7()
        {
            // 2oo3, all different

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var et1 = system.add_element_type(new ElementType(
                0, 0.01, 0, 0, 0, 0, 0, 1
            ));

            var et2 = system.add_element_type(new ElementType(
                0, 0.02, 0, 0, 0, 0, 0, 1
            ));

            var et3 = system.add_element_type(new ElementType(
                0, 0.03, 0, 0, 0, 0, 0, 1
            ));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et2);
            var e3 = system.add_element(et3);

            var v1 = new Voting((new int[] { e1, e2, e3 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v1);

            system.compile();

            double p1 = exponential_dist(0.01);
            double p2 = exponential_dist(0.02);
            double p3 = exponential_dist(0.03);

            assert_close(system.failure_probability(1), p1 * p2 + p1 * p3 + p2 * p3 - 2 * p1 * p2 * p3, 1e-8);
        }

        static void test8()
        {
            // a--b-|
            //      | 1oo2 -->
            // c----|

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var at = system.add_element_type(new ElementType(
                0, 0.01, 0, 0, 0, 0, 0, 1
            ));

            var bt = system.add_element_type(new ElementType(
                0, 0.02, 0, 0, 0, 0, 0, 1
            ));

            var ct = system.add_element_type(new ElementType(
                0, 0.03, 0, 0, 0, 0, 0, 1
            ));

            var a = system.add_element(at);
            var b = system.add_element(bt);
            var c = system.add_element(ct);

            var v1 = new Voting((new int[] { a, b }).ToList(), (new Voting[] { }).ToList(), 0);
            var v2 = new Voting((new int[] { c }).ToList(), (new Voting[] { v1 }).ToList(), 1);

            system.set_final_voting(v2);

            system.compile();

            double p1 = exponential_dist(0.01);
            double p2 = exponential_dist(0.02);
            double p3 = exponential_dist(0.03);

            double pab = p1 + p2 - p1 * p2;

            assert_close(system.failure_probability(1), pab * p3, 1e-8);
        }

        static void test9()
        {
            // a--b-|
            //      | 2oo2 -->
            // c----|

            SILSystem system = new SILSystem();

            // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore
            var at = system.add_element_type(new ElementType(
                0, 0.01, 0, 0, 0, 0, 0, 1
            ));

            var bt = system.add_element_type(new ElementType(
                0, 0.02, 0, 0, 0, 0, 0, 1
            ));

            var ct = system.add_element_type(new ElementType(
                0, 0.03, 0, 0, 0, 0, 0, 1
            ));

            var a = system.add_element(at);
            var b = system.add_element(bt);
            var c = system.add_element(ct);

            var v1 = new Voting((new int[] { a, b }).ToList(), (new Voting[] { }).ToList(), 0);
            var v2 = new Voting((new int[] { c }).ToList(), (new Voting[] { v1 }).ToList(), 0);

            system.set_final_voting(v2);

            system.compile();

            double p1 = exponential_dist(0.01);
            double p2 = exponential_dist(0.02);
            double p3 = exponential_dist(0.03);

            double pab = p1 + p2 - p1 * p2;

            assert_close(system.failure_probability(1), pab + p3 - pab * p3, 1e-8);
        }

        static void test10()
        {
            SILSystem system = new SILSystem();

            double Ldu1 = 0.0000002;
            double Ldu2 = 0.00000021;
            double Ldu3 = 0.00000019;
            double Ldd = 0.0000018;
            double BetaD = 0.02;
            double Beta1 = 0.02;
            double Beta2 = 0.021;
            double Beta3 = 0.019;
            double Tprf = 35040;
            double PDC = 0.3;
            double MTTR = 0;
            double MRT = 0;

            var et1 = system.add_element_type(new ElementType(Ldd, Ldu1, PDC, BetaD, Beta1, MRT, MTTR, Tprf));
            var et2 = system.add_element_type(new ElementType(Ldd, Ldu2, PDC, BetaD, Beta2, MRT, MTTR, Tprf));
            var et3 = system.add_element_type(new ElementType(Ldd, Ldu3, PDC, BetaD, Beta3, MRT, MTTR, Tprf));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et2);
            var e3 = system.add_element(et3);

            var v2oo3 = new Voting((new int[] { e1, e2, e3 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v2oo3);

            system.compile();

            double t = 123456;
            double p1 = exponential_dist((t % Tprf) * Ldu1 * PDC) + exponential_dist(t * Ldu1 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu1 * PDC) * exponential_dist(t * Ldu1 * (1 - PDC));
            double p2 = exponential_dist((t % Tprf) * Ldu2 * PDC) + exponential_dist(t * Ldu2 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu2 * PDC) * exponential_dist(t * Ldu2 * (1 - PDC));
            double p3 = exponential_dist((t % Tprf) * Ldu3 * PDC) + exponential_dist(t * Ldu3 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu3 * PDC) * exponential_dist(t * Ldu3 * (1 - PDC));

            double p = p1*p2 + p2*p3 + p1*p3 - 2*p1*p2*p3;


            var test_elem = new ElementType(Ldd, Ldu1, PDC, BetaD, Beta1, MRT, MTTR, Tprf);
            assert_close(test_elem.common_failure(t) + test_elem.independent_failure(t) - test_elem.common_failure(t) * test_elem.independent_failure(t), p1, 1e-3);

            assert_close(system.failure_probability(t), p, 3e-3);


            t = 140160;
            p1 = exponential_dist((t % Tprf) * Ldu1 * PDC) + exponential_dist(t * Ldu1 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu1 * PDC) * exponential_dist(t * Ldu1 * (1 - PDC));
            p2 = exponential_dist((t % Tprf) * Ldu2 * PDC) + exponential_dist(t * Ldu2 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu2 * PDC) * exponential_dist(t * Ldu2 * (1 - PDC));
            p3 = exponential_dist((t % Tprf) * Ldu3 * PDC) + exponential_dist(t * Ldu3 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu3 * PDC) * exponential_dist(t * Ldu3 * (1 - PDC));

            p = p1*p2 + p2*p3 + p1*p3 - 2*p1*p2*p3;

            assert_close(system.failure_probability(t), p, 3e-3);
        }

        static void test11()
        {
            SILSystem system = new SILSystem();

            double Ldu1 = 0.0000002;
            double Ldu2 = 0.00000021;
            double Ldu3 = 0.00000019;
            double Ldd = 0.0000018;
            double BetaD = 0.02;
            double Beta1 = 0.02;
            double Beta2 = 0.021;
            double Beta3 = 0.019;
            double Tprf = 35040;
            double PDC = 0.5;
            double MTTR = 100;
            double MRT = 100;

            var et1 = system.add_element_type(new ElementType(Ldd, Ldu1, PDC, BetaD, Beta1, MRT, MTTR, Tprf));
            var et2 = system.add_element_type(new ElementType(Ldd, Ldu2, PDC, BetaD, Beta2, MRT, MTTR, Tprf));
            var et3 = system.add_element_type(new ElementType(Ldd, Ldu3, PDC, BetaD, Beta3, MRT, MTTR, Tprf));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et2);
            var e3 = system.add_element(et3);

            var v2oo3 = new Voting((new int[] { e1, e2, e3 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v2oo3);

            system.compile();

            double t = 123456;
            double p1 = exponential_dist((t % Tprf) * Ldu1 * PDC) + exponential_dist(t * Ldu1 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu1 * PDC) * exponential_dist(t * Ldu1 * (1 - PDC)) + MTTR * Ldu1 + MRT * Ldd;
            double p2 = exponential_dist((t % Tprf) * Ldu2 * PDC) + exponential_dist(t * Ldu2 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu2 * PDC) * exponential_dist(t * Ldu2 * (1 - PDC)) + MTTR * Ldu2 + MRT * Ldd;
            double p3 = exponential_dist((t % Tprf) * Ldu3 * PDC) + exponential_dist(t * Ldu3 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu3 * PDC) * exponential_dist(t * Ldu3 * (1 - PDC)) + MTTR * Ldu3 + MRT * Ldd;

            double p = p1*p2 + p2*p3 + p1*p3 - 2*p1*p2*p3;


            assert_close(system.failure_probability(t), p, 3e-3);


            t = 140160;
            p1 = exponential_dist((t % Tprf) * Ldu1 * PDC) + exponential_dist(t * Ldu1 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu1 * PDC) * exponential_dist(t * Ldu1 * (1 - PDC)) + MTTR * Ldu1 + MRT * Ldd;
            p2 = exponential_dist((t % Tprf) * Ldu2 * PDC) + exponential_dist(t * Ldu2 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu2 * PDC) * exponential_dist(t * Ldu2 * (1 - PDC)) + MTTR * Ldu1 + MRT * Ldd;
            p3 = exponential_dist((t % Tprf) * Ldu3 * PDC) + exponential_dist(t * Ldu3 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu3 * PDC) * exponential_dist(t * Ldu3 * (1 - PDC)) + MTTR * Ldu1 + MRT * Ldd;

            p = p1*p2 + p2*p3 + p1*p3 - 2*p1*p2*p3;

            assert_close(system.failure_probability(t), p, 1e-3);
        }

        static void test12()
        {
            SILSystem system = new SILSystem();

            double Ldu1 = 0.0000002;
            double Ldu2 = 0.00000021;
            double Ldu3 = 0.00000019;
            double Ldd = 0.0000018;
            double BetaD = 0.02;
            double Beta1 = 0.02;
            double Beta2 = 0.021;
            double Beta3 = 0.019;
            double TL = 140160;
            double Tprf = 35040;
            double PDC = 0.5;
            double MTTR = 100;
            double MRT = 100;

            var et1 = system.add_element_type(new ElementType(Ldd, Ldu1, PDC, BetaD, Beta1, MRT, MTTR, Tprf));
            var et2 = system.add_element_type(new ElementType(Ldd, Ldu2, PDC, BetaD, Beta2, MRT, MTTR, Tprf));
            var et3 = system.add_element_type(new ElementType(Ldd, Ldu3, PDC, BetaD, Beta3, MRT, MTTR, Tprf));

            var e1 = system.add_element(et1);
            var e2 = system.add_element(et2);
            var e3 = system.add_element(et3);

            var v2oo3 = new Voting((new int[] { e1, e2, e3 }).ToList(), (new Voting[] { }).ToList(), 1);

            system.set_final_voting(v2oo3);

            system.compile();

            foreach (double t in new double[]{0, 1000, 50000, 100000, 130000})
            {
                double p1 = exponential_dist((t % Tprf) * Ldu1 * PDC) + exponential_dist(t * Ldu1 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu1 * PDC) * exponential_dist(t * Ldu1 * (1 - PDC)) + MTTR * Ldu1 + MRT * Ldd;
                double p2 = exponential_dist((t % Tprf) * Ldu2 * PDC) + exponential_dist(t * Ldu2 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu2 * PDC) * exponential_dist(t * Ldu2 * (1 - PDC)) + MTTR * Ldu2 + MRT * Ldd;
                double p3 = exponential_dist((t % Tprf) * Ldu3 * PDC) + exponential_dist(t * Ldu3 * (1 - PDC)) - exponential_dist((t % Tprf) * Ldu3 * PDC) * exponential_dist(t * Ldu3 * (1 - PDC)) + MTTR * Ldu3 + MRT * Ldd;

                double p = p1*p2 + p2*p3 + p1*p3 - 2*p1*p2*p3;

                assert_close(system.failure_probability(t), p, 0.01);
            }

            foreach (double t in new double[]{0, 1000, 50000, 100000, 130000})
            {
                double p1 = (t % Tprf) * Ldu1 * PDC + t * Ldu1 * (1 - PDC) + MTTR * Ldu1 + MRT * Ldd;
                double p2 = (t % Tprf) * Ldu2 * PDC + t * Ldu2 * (1 - PDC) + MTTR * Ldu2 + MRT * Ldd;
                double p3 = (t % Tprf) * Ldu3 * PDC + t * Ldu3 * (1 - PDC) + MTTR * Ldu3 + MRT * Ldd;
                
                double p = p1*p2 + p2*p3 + p1*p3;

                assert_close(system.failure_probability(t), p, 0.03);
            }
            

            Console.WriteLine("{0}", system.average_failure_probability(TL, 1e-10));
        }
    }
}