// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Web.Http.TestUtil
{
    public class Dumper
    {
        public Dumper()
        {

        }

        public void Dump(ProgramEvaluation pr, string option = null)
        {
            //Student Program 
            Console.WriteLine("Program Result: " /*+ StudentProgram.ProgramCode + "\t " */+ string.Join(",", pr.Explanations.Select(ex => ex.ToString())));


            foreach (var rr in pr.RequirementResults)
            {
                // Requirement
                //Console.WriteLine("\tRequirement: " + rr.Requirement.Code + "\t " + string.Join(",", rr.Explanations.Select(ex => ex.ToString())));
                Console.WriteLine("\tRequirement: " + rr.Requirement.Code + "\t " + " Status: " + rr.CompletionStatus.ToString()
                                                                                  + ",  " + rr.PlanningStatus.ToString());

                foreach (var sr in rr.SubRequirementResults)
                {
                    //Subrequirement
                    //Console.WriteLine("\t\tSubrequirement: " + sr.SubRequirement.Code + "\t " + string.Join(",", sr.Explanations.Select(ex => ex.ToString())));
                    Console.WriteLine("\t\tSubrequirement: " + sr.SubRequirement.Code + "\t " + " Status: " + sr.CompletionStatus.ToString()
                                                                                              + ", " + sr.PlanningStatus.ToString());

                    foreach (var gr in sr.GroupResults)
                    {
                        //Group
                        Console.WriteLine("\t\t\tGroup: " + gr.Group.Id + " " + gr.Group.Code + "\t " + string.Join(",", gr.Explanations.Select(ex => ex.ToString())));


                        // result
                        if (!string.IsNullOrEmpty(option))
                        {
                            foreach (string res in gr.EvalDebug)
                            {
                                if (option.ToLower() != "brief" || res.ToLower().Contains("applied"))
                                {
                                    Console.WriteLine("\t\t\t\t\t" + res); 
                                }
                            }
                        }
                        // gr.Results are actually kind of useless in this context.  The objects are initialized in 
                        // EvaluationContext and then passed around.  The resulting AcadResults are accurate in the context of the
                        // complete evaluation at the end, but not with respect to the groups against which they were evaluated 
                        // before the one to which they were applied.   It's kind of an ugly hack to store the group-level results
                        // in text, but it seems equally silly to have two sets of AcadResults.  The only business reason to store
                        // or display the group-level results is to answer the user's question "why did/didn't this credit/course 
                        // apply to this requirement[group]?"  It is not unreasonable to think the user may at some point want that answered
                        // but we don't have a "user story" for that at the moment.  One hopes that it would be mostly self-explanatory.

                    }

                }

            }
        }


    }
}
